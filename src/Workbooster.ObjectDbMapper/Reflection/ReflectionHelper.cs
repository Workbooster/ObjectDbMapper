using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Workbooster.ObjectDbMapper.Attributes;

namespace Workbooster.ObjectDbMapper.Reflection
{
    public static class ReflectionHelper
    {
        #region PUBLIC METHODS
        
        /// <summary>
        /// Reflects an entity and its field (properties and fields) from the given entity data class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listOfIgnoredFieldNames"></param>
        /// <returns></returns>
        public static EntityDefinition GetEntityDefinitionFromType<T>(IEnumerable<string> listOfIgnoredFieldNames = null)
        {
            EntityDefinition entity = new EntityDefinition();
            entity.EntityType = typeof(T);
            entity.DbTableName = GetDbTableName<T>();

            entity.FieldDefinitions = GetFields<T>(entity, listOfIgnoredFieldNames);

            return entity;
        }

        /// <summary>
        /// Sets the given value on the specified field on the given item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldToSet"></param>
        /// <param name="item"></param>
        /// <param name="value"></param>
        public static void SetValue<T>(this FieldDefinition fieldToSet, T item, object value)
        {

            PropertyInfo property = fieldToSet.MemberInfo as PropertyInfo;

            if (property != null)
            {
                // it's a property
                property.SetValue(item, value, null);
            }
            else
            {
                FieldInfo field = fieldToSet.MemberInfo as FieldInfo;

                if (field != null)
                {
                    // it's a field
                    field.SetValue(item, value);
                }
            }
        }

        #endregion

        #region INTERNAL METHODS

        private static string GetDbTableName<T>()
        {
            // check whether the class is marked with a [Table] attribute
            TableAttribute tblAttribute = typeof(T).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;

            if (tblAttribute != null && !String.IsNullOrEmpty(tblAttribute.Name))
            {
                // get the tablename from the attribute
                return tblAttribute.Name;
            }

            return null;
        }

        private static List<FieldDefinition> GetFields<T>(EntityDefinition entity, IEnumerable<string> listOfIgnoredFieldNames = null)
        {
            List<FieldDefinition> listOfFoundFields = new List<FieldDefinition>();

            // load all properties and fields

            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            List<MemberInfo> listOfPropertiesAndFields = new List<MemberInfo>();
            listOfPropertiesAndFields.AddRange(typeof(T).GetFields(bindingFlags));
            listOfPropertiesAndFields.AddRange(typeof(T).GetProperties(bindingFlags));

            foreach (var member in listOfPropertiesAndFields)
            {
                // check whether an ignore attribite is set
                if (member.GetCustomAttributes(typeof(IgnoreAttribute), true).Count() == null)
                {
                    // check whether the field is on the ignore-list
                    if (listOfIgnoredFieldNames == null || listOfIgnoredFieldNames.Contains(member.Name) == false)
                    {
                        FieldDefinition field = new FieldDefinition();
                        field.MemberInfo = member;
                        field.MemberName = member.Name;
                        field.DbColumnName = member.Name;
                        field.Entity = entity;

                        if (member is PropertyInfo)
                        {
                            field.IsProperty = true;
                            field.MemberType = ((PropertyInfo)member).PropertyType;
                        }
                        else
                        {
                            field.IsProperty = false;
                            field.MemberType = ((FieldInfo)member).FieldType;
                        }

                        // check whether the column is marked with a [Column] attribute
                        ColumnAttribute colAttribute = member.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute;

                        if (colAttribute != null && !String.IsNullOrEmpty(colAttribute.Name))
                        {
                            // set the column name from the attribute
                            field.DbColumnName = colAttribute.Name;

                            field.DbType = colAttribute.DbType;
                        }

                        listOfFoundFields.Add(field);
                    }
                }
            }

            return listOfFoundFields;
        }

        #endregion
    }
}
