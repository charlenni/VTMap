using VTMap.Core.Interfaces;

namespace VTMap.Core.Filter
{
    public class NullFilter : Filter
    {
        public override bool Evaluate(IVectorElement feature)
        {
            return true;
        }
    }
}
