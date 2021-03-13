using VTMap.Core.Interfaces;

namespace VTMap.Core.Filter
{
    public class NotHasIdentifierFilter : Filter
    {
        public NotHasIdentifierFilter()
        {
        }

        public override bool Evaluate(IVectorElement feature)
        {
            return feature != null && string.IsNullOrWhiteSpace(feature.Id);
        }
    }
}
