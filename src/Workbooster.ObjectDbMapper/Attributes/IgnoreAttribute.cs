using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Workbooster.ObjectDbMapper
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class IgnoreAttribute : Attribute
    {
        public IgnoreAttribute() { }
    }
}
