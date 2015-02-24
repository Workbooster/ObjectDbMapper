using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Workbooster.ObjectDbMapper
{
    public static class DataReaderExtensions
    {
        public static IList<T> Read<T>(this DbDataReader reader) where T : new()
        {
            List<T> listOfItems = new List<T>();

            Dictionary<int, MemberInfo> dictOfFoundPropertiesAndFields = reader.GetAvailablePropertiesAndFields<T>();

            while (reader.Read())
            {
                T item = new T();

                foreach (var columnInfo in dictOfFoundPropertiesAndFields)
                {
                    PropertyInfo property = columnInfo.Value as PropertyInfo;

                    if (property != null)
                    {
                        // it's a property
                        property.SetValue(item, reader.ConvertValue(columnInfo.Key, property.PropertyType), null);
                    }
                    else
                    {
                        FieldInfo field = columnInfo.Value as FieldInfo;

                        if (field != null)
                        {
                            // it's a field

                            field.SetValue(item, reader.ConvertValue(columnInfo.Key, field.FieldType));
                        }
                    }


                }

                listOfItems.Add(item);
            }

            return listOfItems;
        }

        private static object ConvertValue(this DbDataReader reader, int index, Type expectedType)
        {
            return Convert.ChangeType(reader.GetValue(index), expectedType);
        }

        /// <summary>
        /// Gets a dictionary with the index of the result column and the MemberInfo of the corresponding property or field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Dictionary<int, MemberInfo> GetAvailablePropertiesAndFields<T>(this DbDataReader reader)
        {
            Dictionary<int, MemberInfo> dictOfFoundPropertiesAndFields = new Dictionary<int, MemberInfo>();

            // load all properties and fields

            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            List<MemberInfo> listOfPropertiesAndFields = new List<MemberInfo>();
            listOfPropertiesAndFields.AddRange(typeof(T).GetFields(bindingFlags));
            listOfPropertiesAndFields.AddRange(typeof(T).GetProperties(bindingFlags));

            foreach (var property in listOfPropertiesAndFields)
            {
                int fieldIndex = -1;
                string fieldName = property.Name;
                
                // check whether the column is marked with a [Column] attribute
                ColumnAttribute colAttribute = property.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute;

                if (colAttribute != null && !String.IsNullOrEmpty(colAttribute.Name))
                {
                    fieldName = colAttribute.Name;
                }

                try
                {
                    // try to find the column by name
                    fieldIndex = reader.GetOrdinal(fieldName);
                }
                catch (IndexOutOfRangeException) { /* ignore */	}

                if (fieldIndex != -1)
                {
                    dictOfFoundPropertiesAndFields.Add(fieldIndex, property);
                }
            }

            return dictOfFoundPropertiesAndFields;
        }
    }
}
