using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using Workbooster.ObjectDbMapper.Reflection;

namespace Workbooster.ObjectDbMapper.Commands
{
    public abstract class MappingCommandBase<T>
    {
        #region MEMBERS

        /// <summary>
        /// Key = ColumnName
        /// Value = Mapping for the insert value
        /// </summary>
        protected Dictionary<string, Func<T, object>> _ColumnMappings = new Dictionary<string, Func<T, object>>();

        #endregion

        #region PROPERTIES

        public DbConnection Connection { get; private set; }
        public EntityDefinition Entity { get; private set; }

        #endregion

        #region PUBLIC METHODS

        public MappingCommandBase(DbConnection connection)
        {
            Connection = connection;
            Entity = ReflectionHelper.GetEntityDefinitionFromType<T>();

            if (String.IsNullOrEmpty(Entity.DbTableName))
            {
                throw new Exception("Couldn't resolve the tablename. Please instantiate the command either with a tablename or add a [Table] attribute to the data class.");
            }
        }

        public MappingCommandBase(DbConnection connection, string tableName)
        {
            Connection = connection;
            Entity = ReflectionHelper.GetEntityDefinitionFromType<T>();
            Entity.DbTableName = tableName;
        }

        /// <summary>
        /// Creates or overwrites a mapping between a database column and a field from the data object.
        /// Example: <code>cmd.Map("TypeName", o => { return o.IsCompany ? "Company" : "Person"; });</code>
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="mappingFunction"></param>
        /// <returns></returns>
        public void Map(string columnName, Func<T, object> mappingFunction)
        {
            _ColumnMappings[columnName] = mappingFunction;
        }

        /// <summary>
        /// Removes the field mapping for the given column.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool RemoveMapping(string columnName)
        {
            return _ColumnMappings.Remove(columnName);
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

                    _ColumnMappings[fieldDefinition.DbColumnName] = func;
                }
            }
        }

        #endregion
    }
}
