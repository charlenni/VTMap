using VTMap.Core.Interfaces;

namespace VTMap.Core.Filter
{
    public interface IFilter
    {
        bool Evaluate(IVectorElement feature);
    }
}
