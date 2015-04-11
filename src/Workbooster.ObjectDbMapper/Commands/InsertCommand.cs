using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Workbooster.ObjectDbMapper.Attributes;

namespace Workbooster.ObjectDbMapper.Commands
{
    public class InsertCommand<T>
    {
        #region MEMBERS

        /// <summary>
        /// Key = ColumnName
        /// Value = Mapping for the return value
        /// </summary>
        private Dictionary<string, Func<T, object>> _FieldMappings;

        /// <summary>
        /// only used to create DbParameters
        /// </summary>
        private DbCommand _FactoryCommand;

        #endregion

        #region PROPERTIES

        public DbConnection Connection { get; private set; }
        public string TableName { get; private set; }
        public bool EnableDynamicMapping { get; private set; }

        #endregion

        #region PUBLIC METHODS

        public InsertCommand(DbConnection connection, string tableName, bool enableDynamicMapping = false)
        {
            Connection = connection;
            TableName = tableName;
            EnableDynamicMapping = enableDynamicMapping;
            _FieldMappings = new Dictionary<string, Func<T, object>>();
            _FactoryCommand = Connection.CreateCommand();
        }

        public InsertCommand(DbConnection connection, bool enableDynamicMapping = false)
        {
            Connection = connection;
            EnableDynamicMapping = enableDynamicMapping;
            _FieldMappings = new Dictionary<string, Func<T, object>>();
            _FactoryCommand = Connection.CreateCommand();

            // check whether the class is marked with a [Table] attribute
            TableAttribute tblAttribute = typeof(T).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;

            if (tblAttribute != null && !String.IsNullOrEmpty(tblAttribute.Name))
            {
                // get the tablename from the attribute
                TableName = tblAttribute.Name;
            }
            else
            {
                throw new Exception("Couldn't resolve the tablename. Please instantiate the InsertCommand either with a tablename or add a [Table] attribute to the data class.");
            }
        }

        public InsertCommand<T> Map(string columnName, Func<T, object> mappingFunction)
        {
            _FieldMappings[columnName] = mappingFunction;
            
            return this;
        }

        public int Execute(IEnumerable<T> listOfItems)
        {
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }

            if (EnableDynamicMapping)
            {
                throw new NotImplementedException("Dynamic mapping hasn't been implemented yet");
            }
            else
            {
                return ExecuteNonDynamic(listOfItems);
            }
        }

        #endregion

        #region INTERNAL METHODS

        private int ExecuteNonDynamic(IEnumerable<T> listOfItems)
        {
            if(_FieldMappings.Count == 0)
                throw new Exception("No field mappings are specified.");

            int numberOfRowsAffected = 0;

            // prepare the SQL INSERT statement
            string columnNames = String.Join(",", _FieldMappings.Keys.ToArray());
            string parameterNames = _FieldMappings.Keys.Aggregate("", (acc, s) => acc += ",@" + s).Remove(0, 1);
            string insertStatement = String.Format("INSERT INTO [{0}] ({1}) VALUES({2})", TableName, columnNames, parameterNames);

            foreach (var item in listOfItems)
            {
                DbCommand cmd = Connection.CreateCommand();
                cmd.CommandText = insertStatement;

                foreach (var mapping in _FieldMappings)
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
