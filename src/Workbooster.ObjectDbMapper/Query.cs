using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using Workbooster.ObjectDbMapper.Filters;
using Workbooster.ObjectDbMapper.Reflection;

namespace Workbooster.ObjectDbMapper
{
    /// <summary>
    /// A query stores all the information used to send an SQL SELECT command to the database.
    /// At the moment the Query is enumerated the SQL command is executed. Until then the query can be configured (adding filters, mappings etc.)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Query<T> : IEnumerable<T> where T : new()
    {
        #region MEMBERS

        /// <summary>
        /// only used to create DbParameters
        /// </summary>
        private DbCommand _FactoryCommand;
        private Dictionary<string, Action<T, object>> _ManualMappings = new Dictionary<string, Action<T, object>>();

        #endregion

        #region PROPERTIES

        public DbConnection Connection { get; set; }
        public DbTransaction Transaction { get; set; }
        public string BaseSql { get; set; }
        public EntityDefinition Entity { get; private set; }
        public List<DbParameter> Parameters { get; set; }
        public List<IFilter> Filters { get; set; }

        #endregion

        #region PUBLIC METHODS

        public Query(DbConnection connection)
        {
            Connection = connection;
            Entity = ReflectionHelper.GetEntityDefinitionFromType<T>();

            if (String.IsNullOrEmpty(Entity.DbTableName))
            {
                throw new Exception("Couldn't resolve the tablename. Please instantiate the Query either with a SQL SELECT statement or add a [Table] attribute to the data class.");
            }

            BaseSql = String.Format("SELECT * FROM {0}", Entity.DbTableName);

            Parameters = new List<DbParameter>();
            Filters = new List<IFilter>();
            _FactoryCommand = Connection.CreateCommand();
        }

        public Query(DbConnection connection, string baseSql)
        {
            Connection = connection;
            BaseSql = baseSql;
            Entity = ReflectionHelper.GetEntityDefinitionFromType<T>();
            Parameters = new List<DbParameter>();
            Filters = new List<IFilter>();
            _FactoryCommand = Connection.CreateCommand();
        }

        public IEnumerator<T> GetEnumerator()
        {
            string sql = BaseSql;

            if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }

            DbCommand sqlCmd = GetDbCommand(Connection);

            // use the transaction if one is specified 
            if (this.Transaction != null)
                sqlCmd.Transaction = this.Transaction;

            DbDataReader reader = sqlCmd.ExecuteReader();

            return Read(reader);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Manually map a column from the sql query result.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public Query<T> Map(string columnName, Action<T, object> action)
        {
            this._ManualMappings.Add(columnName, action);

            return this;
        }

        /// <summary>
        /// Append a filter for the where condition.
        /// For each filter a sub-query is added that filters the result of the recent sub-query.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Query<T> Where(IFilter filter)
        {
            this.Filters.Add(filter);

            return this;
        }

        public string GetSql()
        {
            string sql = "";

            sql += BaseSql;

            if (Filters != null && Filters.Count > 0)
            {
                SqlFilterBuilder filterBuilder = new SqlFilterBuilder(Connection, Entity);
                int subCounter = 0;

                foreach (var filter in Filters)
                {
                    // create a new sub-query for each filter/filter group
                    subCounter++;
                    string filterSql = filterBuilder.GetFilterText(filter);

                    if (string.IsNullOrEmpty(filterSql) == false)
                    {
                        filterSql = string.Format("WHERE ({0})", filterSql);
                    }
                    else
                    {
                        filterSql = "";
                    }

                    sql = String.Format("SELECT sub{0}.* FROM ({1}) AS sub{0} {2}", subCounter, sql, filterSql);
                }

                Parameters.AddRange(filterBuilder.Parameters);
            }

            return sql;
        }

        /// <summary>
        /// Creates a DbCommand based on the BaseSql, the Parameters and the Filters.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public DbCommand GetDbCommand(DbConnection connection)
        {
            string sql = GetSql();

            DbCommand sqlCmd = Connection.CreateCommand();

            // use the transaction if one is specified 
            if (this.Transaction != null)
                sqlCmd.Transaction = this.Transaction;

            if (Parameters != null && Parameters.Count > 0)
            {
                sqlCmd.Parameters.AddRange(Parameters.ToArray());
            }

            sqlCmd.CommandText = sql;

            return sqlCmd;
        }

        #endregion

        #region INTERNAL METHODS

        private IEnumerator<T> Read(DbDataReader reader)
        {
            if (typeof(T).IsAssignableFrom(typeof(Record)))
            {
                // handle the request for an anonymous record data type

                try
                {
                    while (reader.Read())
                    {
                        T item = new T();
                        Record record = item as Record;

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string name = reader.GetName(i);
                            object value = reader.GetValue(i);

                            if (value == DBNull.Value)
                            {
                                record.Add(name, null);
                            }
                            else
                            {
                                record.Add(name, value);
                            }
                        }

                        yield return item;
                    }
                }
                finally
                {
                    if (reader != null && reader.IsClosed == false)
                        reader.Close();
                }
            }
            else
            {
                // handle the request for a data class

                Dictionary<int, FieldDefinition> dictOfFoundPropertiesAndFields = GetAvailablePropertiesAndFields(reader);

                try
                {
                    while (reader.Read())
                    {
                        T item = new T();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string name = reader.GetName(i);
                            object value = reader.GetValue(i);

                            if (_ManualMappings.ContainsKey(name))
                            {
                                // A manual mapping for this column is specified

                                try
                                {
                                    // Execute the mapping function.
                                    // Specify the the current instance of the data class and the current value as parameters.

                                    value = value == DBNull.Value ? null : value; // exchange DBNull with null
                                    _ManualMappings[name](item, value);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(String.Format(
                                        "Error while trying to assign the value '{0}' from the column '{1}' by using manual mapping. Message: {2}",
                                        value, name, ex.Message), ex);
                                }
                            }
                            else if(dictOfFoundPropertiesAndFields.ContainsKey(i))
                            {
                                // Automatically assign the value to a property or a field of the data class

                                var columnInfo = dictOfFoundPropertiesAndFields[i];
                                object convertedValue = ConvertValue(value, columnInfo);
                                columnInfo.SetValue<T>(item, convertedValue);
                            }
                        }

                        yield return item;
                    }
                }
                finally
                {
                    if (reader != null && reader.IsClosed == false)
                        reader.Close();
                }
            }
        }

        private object ConvertValue(object value, FieldDefinition field)
        {
            // check for null value in combination with not nullable types

            if ((field.MemberType.IsValueType && Nullable.GetUnderlyingType(field.MemberType) == null)
                && (value == null || value is DBNull))
            {
                throw new Exception(String.Format(
                    "Error while trying to assign a null value on the field '{0}' in class '{1}'. Please use a nullable type or change the database column to not nullable.",
                    field.MemberName, field.Entity.TypeName));
            }

            if (value == null || value is DBNull)
            {
                return null;
            }
            else
            {
                // if the value is not null try to convert into the expected type

                // get the type for the conversion (not nullable)
                Type notNullableType = Nullable.GetUnderlyingType(field.MemberType) ?? field.MemberType;

                return Convert.ChangeType(value, notNullableType);
            }
        }

        /// <summary>
        /// Gets a dictionary with the index of the result column and the field definition.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Dictionary<int, FieldDefinition> GetAvailablePropertiesAndFields(DbDataReader reader)
        {
            Dictionary<int, FieldDefinition> dictOfFoundPropertiesAndFields = new Dictionary<int, FieldDefinition>();

            foreach (var item in Entity.FieldDefinitions)
            {
                int fieldIndex = -1;

                try
                {
                    // try to find the column by name
                    fieldIndex = reader.GetOrdinal(item.DbColumnName);
                }
                catch (IndexOutOfRangeException) { /* ignore */	}

                if (fieldIndex != -1)
                {
                    dictOfFoundPropertiesAndFields.Add(fieldIndex, item);
                }
            }

            return dictOfFoundPropertiesAndFields;
        }

        #endregion
    }
}
