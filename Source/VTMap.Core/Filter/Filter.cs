using VTMap.Core.Interfaces;

namespace VTMap.Core.Filter
{
    public abstract class Filter : IFilter
    {
        public abstract bool Evaluate(IVectorElement feature);
    }
}
