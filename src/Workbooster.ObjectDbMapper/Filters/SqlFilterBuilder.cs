using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Workbooster.ObjectDbMapper.Reflection;

namespace Workbooster.ObjectDbMapper.Filters
{
    public class SqlFilterBuilder
    {
        #region MEMBERS

        /// <summary>
        /// only used to create DbParameters
        /// </summary>
        private DbCommand _FactoryCommand;

        #endregion

        #region PROPERTIES

        public DbConnection Connection { get; set; }

        /// <summary>
        /// Is optional and can be left empty.
        /// </summary>
        public EntityDefinition Entity { get; private set; }

        public List<DbParameter> Parameters { get; set; }

        #endregion

        #region PUBLIC METHODS

        public SqlFilterBuilder(DbConnection connection, EntityDefinition entity = null)
        {
            Connection = connection;
            Entity = entity;

            Parameters = new List<DbParameter>();
            _FactoryCommand = Connection.CreateCommand();
        }

        public string GetFilterText(IFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

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

        #endregion

        #region INTERNAL METHODS

        private string GetFilterText(FilterComparison filter)
        {
            if (filter == null || String.IsNullOrEmpty(filter.FieldName))
                return "";

            // catch null values

            if (filter.Value == null)
            {
                if (filter.Operator == FilterComparisonOperatorEnum.Equal
                    || filter.Operator == FilterComparisonOperatorEnum.ExactlyEqual)
                {
                    return String.Format(" {0} IS NULL", Connection.EscapeObjectName(filter.FieldName));
                }
                else if (filter.Operator == FilterComparisonOperatorEnum.NotEqual
                          || filter.Operator == FilterComparisonOperatorEnum.ExactlyNotEqual)
                {
                    return String.Format(" {0} IS NOT NULL", Connection.EscapeObjectName(filter.FieldName));
                }
            }

            FieldDefinition field = null;

            if (Entity != null
                && Entity.EntityType.IsAssignableFrom(typeof(Record)))
            {
                // an entity definition is available
                // try to find the filed

                field = Entity.FieldDefinitions.Where(f => f.DbColumnName == filter.FieldName).FirstOrDefault();
            }

            if (field != null)
            {
                // handle the request for a known field in the current entity

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
                    // get the type for the conversion (not nullable)
                    Type notNullableType = Nullable.GetUnderlyingType(field.MemberType) ?? field.MemberType;

                    // convert the value to the type of the member
                    param.Value = Convert.ChangeType(filter.Value, notNullableType);
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
                    text = String.Format(" {0} = @{1} {2}",
                        Connection.EscapeObjectName(filter.FieldName),
                        param.ParameterName,
                        GetCaseSensivityCollation());
                }
                else if (field.MemberType == typeof(string)
                  && filter.Operator == FilterComparisonOperatorEnum.ExactlyNotEqual)
                {
                    text = String.Format(" {0} <> @{1} {2}",
                        Connection.EscapeObjectName(filter.FieldName),
                        param.ParameterName,
                        GetCaseSensivityCollation());
                }
                else
                {
                    text = String.Format(" {0} {1} @{2}",
                        Connection.EscapeObjectName(filter.FieldName),
                        FilterUtilities.GetSqlComparisonOperator(filter.Operator),
                        param.ParameterName);
                }

                return text;
            }
            else
            {
                // handle the request for an unknow filed, an unknown entity type or an anonymous record data type

                string text = "";

                // create a SQL parameter

                DbParameter param = _FactoryCommand.CreateParameter();

                // generate a unique parameter name from a GUID
                param.ParameterName = Guid.NewGuid().ToString().Replace('-', '_');
                param.Value = filter.Value == null ? DBNull.Value : filter.Value;

                // add the parameter to the local collection
                Parameters.Add(param);

                if (filter.Value is string
                    && filter.Operator == FilterComparisonOperatorEnum.ExactlyEqual)
                {
                    text = String.Format(" {0} = @{1} {2}",
                        Connection.EscapeObjectName(filter.FieldName),
                        param.ParameterName,
                        GetCaseSensivityCollation());
                }
                else if (filter.Value is string
                  && filter.Operator == FilterComparisonOperatorEnum.ExactlyNotEqual)
                {
                    text = String.Format(" {0} <> @{1} {2}",
                        Connection.EscapeObjectName(filter.FieldName),
                        param.ParameterName,
                        GetCaseSensivityCollation());
                }
                else
                {
                    text = String.Format(" {0} {1} @{2}",
                        Connection.EscapeObjectName(filter.FieldName),
                        FilterUtilities.GetSqlComparisonOperator(filter.Operator),
                        param.ParameterName);
                }

                return text;
            }
        }

        private string GetCaseSensivityCollation()
        {
            switch (Connection.GetDatabaseType())
            {
                case DatabaseEngineEnum.MSSQL:
                    return "COLLATE sql_latin1_general_cp1_cs_as";
                case DatabaseEngineEnum.MySQL:
                    return "COLLATE latin1_general_cs";
                default:
                    return "";
            }
        }

        private string GetFilterText(FilterGroup group)
        {
            string sql = "";
            bool isFirst = true;

            if (group.Filters != null && group.Filters.Count > 0)
            {
                foreach (var filter in group.Filters)
                {
                    // catch empty filter groups
                    if ((filter is FilterGroup && (((FilterGroup)filter).Filters == null || ((FilterGroup)filter).Filters.Count == 0)) == false)
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
                }
            }

            return sql;
        }

        #endregion
    }
}
