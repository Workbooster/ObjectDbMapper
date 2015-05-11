using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Workbooster.ObjectDbMapper
{
    /// <summary>
    /// Is used if no data class is available. Instead of fields and properties the values are stored in a dictionary.
    /// </summary>
    public class Record : Dictionary<string, object>
    {
        #region PUBLIC METHODS

        /// <summary>
        /// Gets a list of key-value pairs of type string.
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> ToListOfKeyValueStringPairs()
        {
            return this.Select(v => new KeyValuePair<string, string>(v.Key, v.Value.ToString())).ToList();
        }

        #endregion
    }
}
