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
    public class Query<T> : IEnumerable<T> where T : new()
    {
        #region MEMBERS

        /// <summary>
        /// only used to create DbParameters
        /// </summary>
        private DbCommand _FactoryCommand;

        #endregion

        #region PROPERTIES

        public DbConnection Connection { get; set; }
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

            DbDataReader reader = sqlCmd.ExecuteReader();

            return Read(reader);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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
                int subCounter = 0;

                foreach (var filter in Filters)
                {
                    // create a new sub-query for each filter/filter group
                    subCounter++;
                    string filterSql = GetFilterText(filter);
                    sql = String.Format("SELECT sub{0}.* FROM ({1}) AS sub{0} WHERE ({2})", subCounter, sql, filterSql);
                }
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
            Dictionary<int, FieldDefinition> dictOfFoundPropertiesAndFields = GetAvailablePropertiesAndFields(reader);

            while (reader.Read())
            {
                T item = new T();

                foreach (var columnInfo in dictOfFoundPropertiesAndFields)
                {
                    object value = ConvertValue(reader, columnInfo.Key, columnInfo.Value);
                    columnInfo.Value.SetValue<T>(item, value);
                }

                yield return item;
            }
        }

        private object ConvertValue(DbDataReader reader, int index, FieldDefinition field)
        {
            // read the value
            object value = reader.GetValue(index);

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

        private string GetFilterText(IFilter filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");

            if (filter is FilterGroup)
            {
                return GetFilterText((FilterGroup)filter);
            }
            else if (filter is FilterComparison)
            {
                return GetFilterText((FilterComparison)filter);
            }

            throw new Exception(String.Format("Unknown filter type: '{0}'", filter.GetType().Name));
        }

        private string GetFilterText(FilterComparison filter)
        {
            FieldDefinition field = Entity.FieldDefinitions.Where(f => f.DbColumnName == filter.FieldName).FirstOrDefault();

            if (field != null)
            {
                string text = "";

                // create a SQL parameter

                DbParameter param = _FactoryCommand.CreateParameter();

                // generate a unique parameter name from a GUID
                param.ParameterName = Guid.NewGuid().ToString().Replace('-', '_');

                // check whether a different DbType is set
                if (field.DbType != null)
                {
                    param.DbType = (DbType)field.DbType;
                }

                try
                {
                    // convert the value to the type of the member
                    param.Value = Convert.ChangeType(filter.Value, field.MemberType);
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format(
                        "Error while converting the value '{0}' into '{1}' while creating a filter for the field '{2}'.",
                        filter.Value, field.MemberType.Name, filter.FieldName), ex);
                }

                // add the parameter to the local collection
                Parameters.Add(param);

                if (field.MemberType == typeof(string)
                    && filter.Operator == FilterComparisonOperatorEnum.ExactlyEqual)
                {
                    text = String.Format(" [{0}] = @{1} COLLATE sql_latin1_general_cp1_cs_as",
                        filter.FieldName,
                        param.ParameterName);
                }
                else if (field.MemberType == typeof(string)
                  && filter.Operator == FilterComparisonOperatorEnum.ExactlyEqual)
                {
                    text = String.Format(" [{0}] <> @{1} COLLATE sql_latin1_general_cp1_cs_as",
                        filter.FieldName,
                        param.ParameterName);
                }
                else
                {
                    text = String.Format(" [{0}] {1} @{2}",
                        filter.FieldName,
                        FilterUtilities.GetSqlComparisonOperator(filter.Operator),
                        param.ParameterName);
                }

                return text;
            }
            else
            {
                throw new Exception(String.Format("Unknown field: '{0}'", filter.FieldName));
            }
        }

        private string GetFilterText(FilterGroup group)
        {
            string sql = "";
            bool isFirst = true;

            foreach (var filter in group.Filters)
            {
                if (isFirst == true)
                {
                    isFirst = false;
                }
                else
                {
                    sql += " " + FilterUtilities.GetSqlGroupOperator(group.Operatror);
                }

                if (filter is FilterGroup)
                {
                    sql += "(" + GetFilterText((FilterGroup)filter) + ")";
                }
                else if (filter is FilterComparison)
                {
                    sql += GetFilterText((FilterComparison)filter);
                }
                else
                {
                    throw new Exception(String.Format("Unknown filter: '{0}'", filter.GetType().Name));
                }
            }

            return sql;
        }

        #endregion
    }
}
