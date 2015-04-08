using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Workbooster.ObjectDbMapper
{
    public class FilterComparison : IFilter
    {
        public string FieldName;
        public object Value;
        public FilterComparisonOperatorEnum Operator;

        public FilterComparison() {}

        public FilterComparison(string fieldName, FilterComparisonOperatorEnum op, object value)
        {
            FieldName = fieldName;
            Operator = op;
            Value = value;
        }
    }
}
