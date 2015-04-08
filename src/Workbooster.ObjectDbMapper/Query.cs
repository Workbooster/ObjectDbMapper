using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Workbooster.ObjectDbMapper
{
    public class Query<T> : IEnumerable<T> where T : new()
    {
        #region MEMBERS

        private Dictionary<string, MemberInfo> _PropertiesAndFields;

        /// <summary>
        /// only used to create DbParameters
        /// </summary>
        private DbCommand _FactoryCommand;

        #endregion

        #region PROPERTIES

        public DbConnection Connection { get; set; }
        public string BaseSql { get; set; }
        public List<DbParameter> Parameters { get; set; }
        public List<IFilter> Filters { get; set; }

        #endregion

        #region PUBLIC METHODS

        public Query(DbConnection connection, string baseSql)
        {
            Connection = connection;
            BaseSql = baseSql;
            Parameters = new List<DbParameter>();
            Filters = new List<IFilter>();
            _PropertiesAndFields = GetAllPropertiesAndFields();
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
            string className = typeof(T).Name;
            Dictionary<int, MemberInfo> dictOfFoundPropertiesAndFields = GetAvailablePropertiesAndFields(reader);

            while (reader.Read())
            {
                T item = new T();

                foreach (var columnInfo in dictOfFoundPropertiesAndFields)
                {
                    PropertyInfo property = columnInfo.Value as PropertyInfo;

                    if (property != null)
                    {
                        // it's a property
                        property.SetValue(item, ConvertValue(reader, columnInfo.Key, property.PropertyType, className, property.Name), null);
                    }
                    else
                    {
                        FieldInfo field = columnInfo.Value as FieldInfo;

                        if (field != null)
                        {
                            // it's a field

                            field.SetValue(item, ConvertValue(reader, columnInfo.Key, field.FieldType, className, field.Name));
                        }
                    }


                }

                yield return item;
            }
        }

        private object ConvertValue(DbDataReader reader, int index, Type expectedType, string className, string fieldName)
        {
            // read the value
            object value = reader.GetValue(index);

            // check for null value in combination with not nullable types

            if ((expectedType.IsValueType && Nullable.GetUnderlyingType(expectedType) == null)
                && (value == null || value is DBNull))
            {
                throw new Exception(String.Format(
                    "Error while trying to assign a null value on the field '{0}' in class '{1}'. Please use a nullable type or change the database column to not nullable.",
                    fieldName, className));
            }

            if (value == null || value is DBNull)
            {
                return null;
            }
            else
            {
                // if the value is not null try to convert into the expected type

                // get the type for the conversion (not nullable)
                Type notNullableType = Nullable.GetUnderlyingType(expectedType) ?? expectedType;

                return Convert.ChangeType(value, notNullableType);
            }
        }

        /// <summary>
        /// Gets a dictionary with the index of the result column and the MemberInfo of the corresponding property or field.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Dictionary<int, MemberInfo> GetAvailablePropertiesAndFields(DbDataReader reader)
        {
            Dictionary<int, MemberInfo> dictOfFoundPropertiesAndFields = new Dictionary<int, MemberInfo>();

            foreach (var item in _PropertiesAndFields)
            {
                int fieldIndex = -1;

                try
                {
                    // try to find the column by name
                    fieldIndex = reader.GetOrdinal(item.Key);
                }
                catch (IndexOutOfRangeException) { /* ignore */	}

                if (fieldIndex != -1)
                {
                    dictOfFoundPropertiesAndFields.Add(fieldIndex, item.Value);
                }
            }

            return dictOfFoundPropertiesAndFields;
        }

        /// <summary>
        /// Gets a dictionary with the index of the result column and the MemberInfo of the corresponding property or field.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Dictionary<string, MemberInfo> GetAllPropertiesAndFields()
        {
            Dictionary<string, MemberInfo> dictOfFoundPropertiesAndFields = new Dictionary<string, MemberInfo>();

            // load all properties and fields

            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            List<MemberInfo> listOfPropertiesAndFields = new List<MemberInfo>();
            listOfPropertiesAndFields.AddRange(typeof(T).GetFields(bindingFlags));
            listOfPropertiesAndFields.AddRange(typeof(T).GetProperties(bindingFlags));

            foreach (var property in listOfPropertiesAndFields)
            {
                string fieldName = property.Name;

                // check whether the column is marked with a [Column] attribute
                ColumnAttribute colAttribute = property.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute;

                if (colAttribute != null && !String.IsNullOrEmpty(colAttribute.Name))
                {
                    fieldName = colAttribute.Name;
                }


                dictOfFoundPropertiesAndFields.Add(fieldName, property);
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
            if (_PropertiesAndFields.ContainsKey(filter.FieldName))
            {
                string text = "";
                Type memberType = typeof(string);
                DbType? dbType = null;
                var memberInfo = _PropertiesAndFields[filter.FieldName];

                // get the datatype of the field/property

                if (memberInfo is PropertyInfo)
                {
                    memberType = ((PropertyInfo)memberInfo).PropertyType;
                }
                else if (memberInfo is FieldInfo)
                {
                    memberType = ((FieldInfo)memberInfo).FieldType;
                }

                // check whether a different DbType is set

                // check whether the column is marked with a [Column] attribute
                ColumnAttribute colAttribute = memberInfo.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute;

                if (colAttribute != null)
                {
                    dbType = colAttribute.DbType;
                }

                // create a SQL parameter

                DbParameter param = _FactoryCommand.CreateParameter();

                // generate a unique parameter name from a GUID
                param.ParameterName = Guid.NewGuid().ToString().Replace('-', '_');

                if (dbType != null)
                {
                    param.DbType = (DbType)dbType;
                }

                try
                {
                    // convert the value to the type of the member
                    param.Value = Convert.ChangeType(filter.Value, memberType);
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format(
                        "Error while converting the value '{0}' into '{1}' while creating a filter for the field '{2}'.",
                        filter.Value, memberType.Name, filter.FieldName), ex);
                }

                // add the parameter to the local collection
                Parameters.Add(param);

                if (memberType == typeof(string)
                    && filter.Operator == FilterComparisonOperatorEnum.ExactlyEqual)
                {
                    text = String.Format(" [{0}] = @{1} COLLATE sql_latin1_general_cp1_cs_as",
                        filter.FieldName,
                        param.ParameterName);
                }
                else if (memberType == typeof(string)
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
                        GetComparisonOperatorSql(filter.Operator),
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
                    sql += " " + GetGroupOperatorSql(group.Operatror);
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

        private string GetGroupOperatorSql(FilterGroupOperatorEnum op)
        {
            switch (op)
            {
                case FilterGroupOperatorEnum.And:
                    return "AND";
                case FilterGroupOperatorEnum.Or:
                    return " OR";
                default:
                    throw new Exception(
                        String.Format("Unknown group operator: '{0}'",
                            Enum.GetName(typeof(FilterGroupOperatorEnum), op)));
            }
        }

        private string GetComparisonOperatorSql(FilterComparisonOperatorEnum op)
        {
            switch (op)
            {
                case FilterComparisonOperatorEnum.Equal:
                    return "LIKE";
                case FilterComparisonOperatorEnum.NotEqual:
                    return "NOT LIKE";
                case FilterComparisonOperatorEnum.ExactlyEqual:
                    return "=";
                case FilterComparisonOperatorEnum.ExactlyNotEqual:
                    return "<>";
                case FilterComparisonOperatorEnum.GreaterThan:
                    return ">";
                case FilterComparisonOperatorEnum.GreaterThanOrEqual:
                    return ">=";
                case FilterComparisonOperatorEnum.LessThan:
                    return "<";
                case FilterComparisonOperatorEnum.LessThanOrEqual:
                    return "<=";
                case FilterComparisonOperatorEnum.Like:
                    return "LIKE";
                default:
                    throw new Exception(
                        String.Format("Unknown comparison operator: '{0}'",
                            Enum.GetName(typeof(FilterComparisonOperatorEnum), op)));
            }
        }

        #endregion
    }
}
