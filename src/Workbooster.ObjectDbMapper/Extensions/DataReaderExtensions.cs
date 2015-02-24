using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Workbooster.ObjectDbMapper
{
    public static class DataReaderExtensions
    {
        public static IList<T> Read<T>(this DbDataReader reader) where T : new()
        {
            List<T> listOfItems = new List<T>();

            Dictionary<int, PropertyInfo> dictOfAvailableProperties = reader.GetAvailableProperties<T>();

            while (reader.Read())
            {
                T item = new T();

                foreach (var field in dictOfAvailableProperties)
                {
                    field.Value.SetValue(item, Convert.ChangeType(reader.GetValue(field.Key), field.Value.PropertyType));
                }

                listOfItems.Add(item);
            }

            return listOfItems;
        }

        /// <summary>
        /// Gets a dictionary with the index of the result column and the PropertyInfo of the corresponding type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Dictionary<int, PropertyInfo> GetAvailableProperties<T>(this DbDataReader reader)
        {
            Dictionary<int, PropertyInfo> dictOfAvailableProperties = new Dictionary<int, PropertyInfo>();
            PropertyInfo[] listOfAllProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in listOfAllProperties)
            {
                int fieldIndex = -1;
                string fieldName = property.Name;

                // check whether the column is marked with a [Column] attribute
                ColumnAttribute colAttribute = property.GetCustomAttribute<ColumnAttribute>();

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
                    dictOfAvailableProperties.Add(fieldIndex, property);
                }
            }

            return dictOfAvailableProperties;
        }
    }
}
