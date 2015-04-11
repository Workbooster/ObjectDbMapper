using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Workbooster.ObjectDbMapper.Reflection
{
    public class EntityDefinition
    {
        #region PROPERTIES

        public Type EntityType { get; set; }
        public string TypeName { get { return EntityType == null ? null : EntityType.Name; } }
        public string DbTableName { get; set; }
        public List<FieldDefinition> FieldDefinitions { get; set; }

        #endregion

        #region PUBLIC METHODS

        public EntityDefinition()
        {
            FieldDefinitions = new List<FieldDefinition>();
        }

        #endregion
    }
}
