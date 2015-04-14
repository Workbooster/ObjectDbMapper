﻿using System;
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
