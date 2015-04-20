using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Workbooster.ObjectDbMapper
{
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
