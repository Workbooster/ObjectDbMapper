using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Workbooster.ObjectDbMapper
{
    public class FilterGroup : IFilter
    {
        public List<IFilter> Filters;
        public FilterGroupOperatorEnum Operatror;

        public FilterGroup()
        {
            Filters = new List<IFilter>();
            Operatror = FilterGroupOperatorEnum.And; // set default
        }

        public FilterGroup(FilterGroupOperatorEnum op)
        {
            Filters = new List<IFilter>();
            Operatror = op;
        }

        /// <summary>
        /// Chainable add method for filter values. Example:
        /// <code>
        /// var grp = FilterGroup.New()
        ///    .Add("Firstname", FilterComparisonOperatorEnum.Equal, "Roger")
        ///    .Add("Lastname", FilterComparisonOperatorEnum.Equal, "Guillet");
        /// </code>
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        /// <returns>The current filter group.</returns>
        public FilterGroup Add(string fieldName, FilterComparisonOperatorEnum op, object value)
        {
            this.Filters.Add(new FilterComparison(fieldName, op, value));

            return this;
        }

        /// <summary>
        /// Chainable add method for nested filter groups.
        /// </summary>
        /// <param name="nestedGroup"></param>
        /// <returns>The outer filter group (not the nested group!).</returns>
        public FilterGroup Add(FilterGroup nestedGroup)
        {
            this.Filters.Add(nestedGroup);

            return this;
        }

        /// <summary>
        /// Instantiates a new filter group.
        /// </summary>
        /// <param name="op">default=And</param>
        /// <returns>A new instance.</returns>
        public static FilterGroup New(FilterGroupOperatorEnum op = FilterGroupOperatorEnum.And)
        {
            return new FilterGroup(op);
        }
    }
}
