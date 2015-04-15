﻿using System;
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
        public EntityDefinition Entity { get; private set; }
        public List<DbParameter> Parameters { get; set; }

        #endregion

        #region PUBLIC METHODS

        public SqlFilterBuilder(DbConnection connection, EntityDefinition entity)
        {
            Connection = connection;
            Entity = entity;

            Parameters = new List<DbParameter>();
            _FactoryCommand = Connection.CreateCommand();
        }

        public string GetFilterText(IFilter filter)
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

        #endregion

        #region INTERNAL METHODS

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