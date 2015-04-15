using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Workbooster.ObjectDbMapper.Filters;
using Workbooster.ObjectDbMapper.Reflection;

namespace Workbooster.ObjectDbMapper.Commands
{
    public class UpdateCommand<T> : MappingCommandBase<T>
    {
        #region MEMBERS

        protected Dictionary<string, Func<T, object>> _KeyMappings = new Dictionary<string, Func<T, object>>();

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets or sets a static filter condition that must be matched by all updated records.
        /// </summary>
        public IFilter Filter { get; set; }

        #endregion

        #region PUBLIC METHODS

        public UpdateCommand(DbConnection connection) : base(connection) { }

        public UpdateCommand(DbConnection connection, string tableName) : base(connection, tableName) { }

        /// <summary>
        /// Creates or overwrites a conditional mapping between a database column and a field from the data object.
        /// This is used to build the WHERE command.
        /// Example: <code>cmd.MapKey("Id", o => { return o.Id });</code> leads to <code>WHERE Id = @Value</code>
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="mappingFunction"></param>
        public void MapKey(string columnName, Func<T, object> mappingFunction)
        {
            _KeyMappings[columnName] = mappingFunction;
        }

        public int Execute(T item)
        {
            return Execute(new T[] { item });
        }

        public int Execute(IEnumerable<T> listOfItems)
        {
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }

            if (_ColumnMappings.Count == 0)
                throw new Exception("No field mappings are specified.");

            int numberOfRowsAffected = 0;

            // get the static where conditions from "Filter" property
            List<DbParameter> staticWhereParameters = new List<DbParameter>();
            string staticWhereConditions = GetStaticSqlWhereConditions(ref staticWhereParameters);

            // prepare the SQL UPDATE statement
            string columnMappings = GetColumnMappings();
            string updateCommand = String.Format("UPDATE [{0}] SET {1}", Entity.DbTableName, columnMappings);

            foreach (var item in listOfItems)
            {
                DbCommand cmd = Connection.CreateCommand();

                // get the where conditions for the dynamic key columns
                List<DbParameter> keyWhereParameters = new List<DbParameter>();
                string keyWhereConditions = GetKeySqlWhereConditions(ref cmd, item);

                // build the update statement from the UPDATE command and the WHERE conditions
                cmd.CommandText = updateCommand + GetSqlWhereCommand(staticWhereConditions, keyWhereConditions);

                // add the static WHERE condition parameters
                cmd.Parameters.AddRange(staticWhereParameters.ToArray());

                foreach (var mapping in _ColumnMappings)
                {
                    DbParameter param = cmd.CreateParameter();
                    param.ParameterName = mapping.Key;
                    param.Value = mapping.Value(item);

                    cmd.Parameters.Add(param);
                }

                numberOfRowsAffected += cmd.ExecuteNonQuery();
            }

            return numberOfRowsAffected;
        }

        #endregion

        #region INTERNAL METHODS

        private string GetColumnMappings()
        {
            bool isFirst = true;
            string mappings = "";

            foreach (var columnName in _ColumnMappings.Keys)
            {
                // ignore key fileds
                if (_KeyMappings.Keys.Contains(columnName) == false)
                {
                    if (!isFirst) mappings += ","; else isFirst = false;

                    mappings += columnName + "=@" + columnName;
                }
            }

            return mappings;
        }

        private string GetStaticSqlWhereConditions(ref List<DbParameter> dbParameters)
        {
            if (Filter != null)
            {
                SqlFilterBuilder filterBuilder = new SqlFilterBuilder(Connection, Entity);
                dbParameters.AddRange(filterBuilder.Parameters);
                return filterBuilder.GetFilterText(Filter);
            }

            return "";
        }

        private string GetKeySqlWhereConditions(ref DbCommand cmd, T item)
        {
            bool isFirst = true;
            string filterText = "";
            string filterParamName = "";
            const string filterPrefix = "Filter_";

            foreach (var mapping in _KeyMappings)
            {
                if (!isFirst) filterText += " AND "; else isFirst = false;

                filterParamName = filterPrefix + mapping.Key;
                filterText += mapping.Key + "=@" + filterParamName;

                DbParameter param = cmd.CreateParameter();
                param.ParameterName = filterParamName;
                param.Value = mapping.Value(item);

                cmd.Parameters.Add(param);
            }

            return filterText;
        }

        private string GetSqlWhereCommand(string staticConditions, string keyConditions)
        {
            if (String.IsNullOrEmpty(staticConditions) == false
                && String.IsNullOrEmpty(keyConditions) == false)
            {
                return String.Format(" WHERE ({0}) AND ({1})", staticConditions, keyConditions);
            }
            else if (String.IsNullOrEmpty(staticConditions) == false)
            {
                return String.Format(" WHERE ({0})", staticConditions);
            }
            else if (String.IsNullOrEmpty(keyConditions) == false)
            {
                return String.Format(" WHERE ({0})", keyConditions);
            }

            return "";
        }

        #endregion
    }
}
