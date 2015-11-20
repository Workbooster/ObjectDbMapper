using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using Workbooster.ObjectDbMapper.Attributes;
using Workbooster.ObjectDbMapper.Reflection;

namespace Workbooster.ObjectDbMapper.Commands
{
    public class InsertCommand<T> : MappingCommandBase<T>
    {
        #region MEMBERS

        private List<Action<T, object>> _IdentityMappings = new List<Action<T, object>>();

        #endregion

        #region PROPERTIES

        public object LastIdentity { get; private set; }

        public DbTransaction Transaction { get; set; }

        #endregion

        #region PUBLIC METHODS

        public InsertCommand(DbConnection connection) : base(connection) { }

        public InsertCommand(DbConnection connection, string tableName) : base(connection, tableName) { }

        /// <summary>
        /// Creates a new instance of an InsertCommand.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static InsertCommand<T> New(DbConnection connection, string tableName = null)
        {
            if (tableName != null)
            {
                return new InsertCommand<T>(connection, tableName);
            }
            else
            {
                return new InsertCommand<T>(connection);
            }
        }

        public InsertCommand<T> MapIdentity(Action<T, object> mappingFunction)
        {
            _IdentityMappings.Add(mappingFunction);

            return this;
        }

        /// <summary>
        /// Creates or overwrites a mapping between a database column and a field from the data object.
        /// Example: <code>cmd.Map("TypeName", o => { return o.IsCompany ? "Company" : "Person"; });</code>
        /// </summary>
        /// <param name="columnName">Database column name.</param>
        /// <param name="mappingFunction">A function that returns the value of the column.</param>
        public new InsertCommand<T> Map(string columnName, Func<T, object> mappingFunction)
        {
            base.Map(columnName, mappingFunction);

            return this;
        }

        /// <summary>
        /// Creates or overwrites multiple mappings between a database columns and fields from the data object.
        /// </summary>
        /// <param name="listOfMappings">Key = database column name / Value = a function that returns the value of the column.</param>
        public new InsertCommand<T> Map(Dictionary<string, Func<T, object>> listOfMappings)
        {
            base.Map(listOfMappings);

            return this;
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

            if (_ColumnMappings.Count == 0)
                throw new Exception("No field mappings are specified.");

            int numberOfRowsAffected = 0;

            // prepare the SQL INSERT statement
            string columnNames = String.Join(",", _ColumnMappings.Keys.ToArray());
            string parameterNames = _ColumnMappings.Keys.Aggregate("", (acc, s) => acc += ",@" + s).Remove(0, 1);
            string identitySelect = GetIdentitySelect();
            string insertStatement = String.Format("INSERT INTO {0} ({1}) VALUES({2}); {3}", 
                Connection.EscapeObjectName(Entity.DbTableName), 
                columnNames, 
                parameterNames,
                identitySelect);

            foreach (var item in listOfItems)
            {
                DbCommand cmd = Connection.CreateCommand();
                cmd.CommandText = insertStatement;

                // use the transaction if one is specified 
                if (this.Transaction != null)
                    cmd.Transaction = this.Transaction;

                foreach (var mapping in _ColumnMappings)
                {
                    DbParameter param = cmd.CreateParameter();
                    param.ParameterName = mapping.Key;
                    param.Value = mapping.Value(item) ?? DBNull.Value;

                    cmd.Parameters.Add(param);
                }

                // get the last inserted identity
                LastIdentity = cmd.ExecuteScalar();

                // handle the identity mappings
                foreach (var mapping in _IdentityMappings)
                {
                    mapping(item, LastIdentity);
                }

                numberOfRowsAffected++;
            }

            return numberOfRowsAffected;
        }

        #endregion

        #region INTERNAL METHODS

        private string GetIdentitySelect()
        {
            switch (Connection.GetDatabaseType())
            {
                case DatabaseEngineEnum.MSSQL:
                    return "SELECT @@IDENTITY;";
                case DatabaseEngineEnum.MySQL:
                    return "SELECT LAST_INSERT_ID();";
                default:
                    return "";
            }
        }

        #endregion
    }
}
