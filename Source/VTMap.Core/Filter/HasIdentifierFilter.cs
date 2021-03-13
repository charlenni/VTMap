using VTMap.Core.Interfaces;

namespace VTMap.Core.Filter
{
    public class HasIdentifierFilter : Filter
    {
        public HasIdentifierFilter()
        {
        }

        public override bool Evaluate(IVectorElement feature)
        {
            return feature != null && !string.IsNullOrWhiteSpace(feature.Id);
        }
    }
}
