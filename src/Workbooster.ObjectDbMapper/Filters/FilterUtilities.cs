using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Workbooster.ObjectDbMapper.Filters
{
    public static class FilterUtilities
    {
        public static string GetSqlGroupOperator(FilterGroupOperatorEnum op)
        {
            switch (op)
            {
                case FilterGroupOperatorEnum.And:
                    return "AND";
                case FilterGroupOperatorEnum.Or:
                    return " OR";
                default:
                    throw new Exception(
                        String.Format("Unknown group operator: '{0}'",
                            Enum.GetName(typeof(FilterGroupOperatorEnum), op)));
            }
        }

        public static string GetSqlComparisonOperator(FilterComparisonOperatorEnum op)
        {
            switch (op)
            {
                case FilterComparisonOperatorEnum.Equal:
                    return "LIKE";
                case FilterComparisonOperatorEnum.NotEqual:
                    return "NOT LIKE";
                case FilterComparisonOperatorEnum.ExactlyEqual:
                    return "=";
                case FilterComparisonOperatorEnum.ExactlyNotEqual:
                    return "<>";
                case FilterComparisonOperatorEnum.GreaterThan:
                    return ">";
                case FilterComparisonOperatorEnum.GreaterThanOrEqual:
                    return ">=";
                case FilterComparisonOperatorEnum.LessThan:
                    return "<";
                case FilterComparisonOperatorEnum.LessThanOrEqual:
                    return "<=";
                case FilterComparisonOperatorEnum.Like:
                    return "LIKE";
                default:
                    throw new Exception(
                        String.Format("Unknown comparison operator: '{0}'",
                            Enum.GetName(typeof(FilterComparisonOperatorEnum), op)));
            }
        }
    }
}
