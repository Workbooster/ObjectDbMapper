using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Workbooster.ObjectDbMapper.Reflection
{
    public class FieldDefinition
    {
        public string MemberName { get; set; }
        public Type MemberType { get; set; }
        public MemberInfo MemberInfo { get; set; }
        public bool IsProperty { get; set; }
        public string DbColumnName { get; set; }
        public DbType? DbType { get; set; }
        public EntityDefinition Entity { get; set; }
    }
}
