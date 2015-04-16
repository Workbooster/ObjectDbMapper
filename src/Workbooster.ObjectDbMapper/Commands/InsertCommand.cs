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
        #region PUBLIC METHODS

        public InsertCommand(DbConnection connection) : base(connection) { }

        public InsertCommand(DbConnection connection, string tableName) : base(connection, tableName) { }

        /// <summary>
        /// Creates a new instance of an InsertCommand.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public InsertCommand<T> New(DbConnection connection, string tableName = null)
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
            string insertStatement = String.Format("INSERT INTO [{0}] ({1}) VALUES({2})", Entity.DbTableName, columnNames, parameterNames);

            foreach (var item in listOfItems)
            {
                DbCommand cmd = Connection.CreateCommand();
                cmd.CommandText = insertStatement;

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
    }
}
