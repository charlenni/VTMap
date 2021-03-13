using System.Collections.Generic;
using VTMap.Core.Interfaces;

namespace VTMap.Core.Filter
{
    public class NoneFilter : CompoundFilter
    {
        public NoneFilter(List<IFilter> filters) : base(filters)
        {
        }

        public override bool Evaluate(IVectorElement feature)
        {
            foreach (var filter in Filters)
            {
                if (filter.Evaluate(feature))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
