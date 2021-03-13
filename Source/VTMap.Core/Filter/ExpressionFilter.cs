using VTMap.Core.Interfaces;

namespace VTMap.Core.Filter
{
    public class ExpressionFilter : Filter
    {
        public override bool Evaluate(IVectorElement feature)
        {
            return false;
        }
    }
}
