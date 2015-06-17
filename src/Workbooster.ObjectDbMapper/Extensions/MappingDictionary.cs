using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Workbooster.ObjectDbMapper
{
    /// <summary>
    /// Can be used to store/predefine some mappings and use it for example in different InsertCommands or UpdateCommands or as parameter.
    /// </summary>
    public static class MappingDictionary
    {
        public static MappingDictionary<T> New<T>()
        {
            return new MappingDictionary<T>();
        }
    }

    /// <summary>
    /// Can be used to store/predefine some mappings and use it for example in different InsertCommands or UpdateCommands or as parameter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MappingDictionary<T> : Dictionary<string, Func<T, object>>
    {
        public new MappingDictionary<T> Add(string columnName, Func<T, object> mappingFunction)
        {
            return Map(columnName, mappingFunction);
        }

        public MappingDictionary<T> Map(string columnName, Func<T, object> mappingFunction)
        {
            base.Add(columnName, mappingFunction);

            return this;
            
        }
    }
}
