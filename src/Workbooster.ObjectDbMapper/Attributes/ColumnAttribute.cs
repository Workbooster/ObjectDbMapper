using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Workbooster.ObjectDbMapper
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class ColumnAttribute : Attribute
    {
        public string Name { get; set; }
        public DbType? DbType { get; set; }

        public ColumnAttribute(string name)
        {
            Name = name;
        }
    }
}
