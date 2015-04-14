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
    public class InsertCommand<T>
    {
        #region MEMBERS

        /// <summary>
        /// Key = ColumnName
        /// Value = Mapping for the return value
        /// </summary>
        private Dictionary<string, Func<T, object>> _FieldMappings = new Dictionary<string, Func<T, object>>();

        #endregion

        #region PROPERTIES

        public DbConnection Connection { get; private set; }
        public EntityDefinition Entity { get; private set; }

        #endregion

        #region PUBLIC METHODS

        public InsertCommand(DbConnection connection)
        {
            Connection = connection;
            Entity = ReflectionHelper.GetEntityDefinitionFromType<T>();

            if (String.IsNullOrEmpty(Entity.DbTableName))
            {
                throw new Exception("Couldn't resolve the tablename. Please instantiate the InsertCommand either with a tablename or add a [Table] attribute to the data class.");
            }
        }

        public InsertCommand(DbConnection connection, string tableName)
        {
            Connection = connection;
            Entity = ReflectionHelper.GetEntityDefinitionFromType<T>();
            Entity.DbTableName = tableName;
        }

        /// <summary>
        /// Creates a mapping between a database column and a field from the data object.
        /// Example: <code>insert.Map("TypeName", o => { return o.IsCompany ? "Company" : "Person"; });</code>
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="mappingFunction"></param>
        /// <returns></returns>
        public InsertCommand<T> Map(string columnName, Func<T, object> mappingFunction)
        {
            _FieldMappings[columnName] = mappingFunction;

            return this;
        }

        /// <summary>
        /// Removes the field mapping for the given column.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool RemoveMapping(string columnName)
        {
            return _FieldMappings.Remove(columnName);
        }

        /// <summary>
        /// Creates the column-to-field or column-to-property mappings dynamically by using reflection and the attributes.
        /// Give a list with names of fields or properties which should be ignored or use RemoveMapping(string columnName) to remove unneeded columns.
        /// </summary>
        /// <param name="ignoredFieldsOrProperties">A list with names of fields or properties which should be ignored.</param>
        public void CreateDynamicMappings(IEnumerable<string> ignoredFieldsOrProperties = null)
        {
            foreach (var fieldDefinition in Entity.FieldDefinitions)
            {
                if (ignoredFieldsOrProperties == null
                    || ignoredFieldsOrProperties.Contains(fieldDefinition.MemberName) == false)
                {
                    Func<T, object> func = null;

                    if (fieldDefinition.IsProperty)
                    {
                        func = delegate(T o)
                        {
                            return ((PropertyInfo)fieldDefinition.MemberInfo).GetValue(o, null);
                        };
                    }
                    else
                    {
                        func = delegate(T o)
                        {
                            return ((FieldInfo)fieldDefinition.MemberInfo).GetValue(o);
                        };
                    }

                    _FieldMappings.Add(fieldDefinition.DbColumnName, func);
                }
            }
        }

        public int Execute(IEnumerable<T> listOfItems)
        {
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }

            if (_FieldMappings.Count == 0)
                throw new Exception("No field mappings are specified.");

            int numberOfRowsAffected = 0;

            // prepare the SQL INSERT statement
            string columnNames = String.Join(",", _FieldMappings.Keys.ToArray());
            string parameterNames = _FieldMappings.Keys.Aggregate("", (acc, s) => acc += ",@" + s).Remove(0, 1);
            string insertStatement = String.Format("INSERT INTO [{0}] ({1}) VALUES({2})", Entity.DbTableName, columnNames, parameterNames);

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

        #region INTERNAL METHODS

        #endregion
    }
}
