using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Workbooster.ObjectDbMapper.Reflection;

namespace Workbooster.ObjectDbMapper.Commands
{
    public class UpdateCommand<T> : MappingCommandBase<T>
    {
        #region PUBLIC METHODS

        public UpdateCommand(DbConnection connection) : base(connection) { }

        public UpdateCommand(DbConnection connection, string tableName) : base(connection, tableName) { }

        public int Execute(IEnumerable<T> listOfItems)
        {
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }

            if (_ColumnMappings.Count == 0)
                throw new Exception("No field mappings are specified.");

            int numberOfRowsAffected = 0;

            // prepare the SQL UPDATE statement
            string parameterNames = _ColumnMappings.Keys.Aggregate("", (acc, s) => acc += "," + s +  "=@" + s).Remove(0, 1);
            string updateStatement = String.Format("UPDATE [{0}] SET {1}", Entity.DbTableName, parameterNames);

            foreach (var item in listOfItems)
            {
                DbCommand cmd = Connection.CreateCommand();
                cmd.CommandText = updateStatement;

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
