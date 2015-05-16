using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Workbooster.ObjectDbMapper.Filters;
using Workbooster.ObjectDbMapper.Reflection;

namespace Workbooster.ObjectDbMapper.Commands
{
    public class DeleteCommand<T>
    {
        #region MEMBERS

        protected Dictionary<string, Func<T, object>> _KeyMappings = new Dictionary<string, Func<T, object>>();

        #endregion

        #region PROPERTIES

        public DbConnection Connection { get; private set; }
        public EntityDefinition Entity { get; private set; }

        /// <summary>
        /// Gets or sets a static filter condition that must be matched by all deleted records.
        /// </summary>
        public IFilter Filter { get; set; }

        #endregion

        #region PUBLIC METHODS

        public DeleteCommand(DbConnection connection)
        {
            Connection = connection;
            Entity = ReflectionHelper.GetEntityDefinitionFromType<T>();

            if (String.IsNullOrEmpty(Entity.DbTableName))
            {
                throw new Exception("Couldn't resolve the tablename. Please instantiate the command either with a tablename or add a [Table] attribute to the data class.");
            }
        }

        public DeleteCommand(DbConnection connection, string tableName)
        {
            Connection = connection;
            Entity = ReflectionHelper.GetEntityDefinitionFromType<T>();
            Entity.DbTableName = tableName;
        }

        /// <summary>
        /// Creates a new instance of an DeleteCommand.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static DeleteCommand<T> New(DbConnection connection, string tableName = null)
        {
            if (tableName != null)
            {
                return new DeleteCommand<T>(connection, tableName);
            }
            else
            {
                return new DeleteCommand<T>(connection);
            }
        }

        /// <summary>
        /// Creates or overwrites a conditional mapping between a database column and a field from the data object.
        /// This is used to build the WHERE command.
        /// Example: <code>cmd.MapKey("Id", o => { return o.Id });</code> leads to <code>WHERE Id = @Value</code>
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="mappingFunction"></param>
        public DeleteCommand<T> MapKey(string columnName, Func<T, object> mappingFunction)
        {
            _KeyMappings[columnName] = mappingFunction;

            return this;
        }

        public int Execute()
        {
            return Execute(null);
        }

        public T Execute(T item)
        {
            Execute(new T[] { item });
            return item;
        }

        public int Execute(IEnumerable<T> listOfItems)
        {
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }

            int numberOfRowsAffected = 0;

            // get the static where conditions from "Filter" property
            List<DbParameter> staticWhereParameters = new List<DbParameter>();
            string staticWhereConditions = GetStaticSqlWhereConditions(ref staticWhereParameters);

            // prepare the SQL DELETE statement
            string deleteCommand = String.Format("DELETE FROM {0}", Connection.EscapeObjectName(Entity.DbTableName));

            if (listOfItems == null || _KeyMappings.Count == 0)
            {
                // no items or key mappings given (only static filter)
                // only execute the delete command once with the given static filter conditions

                DbCommand cmd = Connection.CreateCommand();

                // build the delete statement from the DELETE command and the WHERE conditions
                cmd.CommandText = deleteCommand + GetSqlWhereCommand(staticWhereConditions, null);

                // add the static WHERE condition parameters
                cmd.Parameters.AddRange(staticWhereParameters.ToArray());

                numberOfRowsAffected += cmd.ExecuteNonQuery();
            }
            else
            {
                foreach (var item in listOfItems)
                {
                    // loop threw all items and work with key contition mappings

                    DbCommand cmd = Connection.CreateCommand();

                    // get the where conditions for the dynamic key columns
                    List<DbParameter> keyWhereParameters = new List<DbParameter>();
                    string keyWhereConditions = GetKeySqlWhereConditions(ref cmd, item);

                    // build the delete statement from the DELETE command and the WHERE conditions
                    cmd.CommandText = deleteCommand + GetSqlWhereCommand(staticWhereConditions, keyWhereConditions);

                    // add the static WHERE condition parameters
                    cmd.Parameters.AddRange(staticWhereParameters.ToArray());

                    numberOfRowsAffected += cmd.ExecuteNonQuery();
                }
            }

            return numberOfRowsAffected;
        }

        #endregion

        #region INTERNAL METHODS

        private string GetStaticSqlWhereConditions(ref List<DbParameter> dbParameters)
        {
            if (Filter != null)
            {
                SqlFilterBuilder filterBuilder = new SqlFilterBuilder(Connection, Entity);
                string filterText = filterBuilder.GetFilterText(Filter);
                dbParameters.AddRange(filterBuilder.Parameters);
                return filterText;
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
