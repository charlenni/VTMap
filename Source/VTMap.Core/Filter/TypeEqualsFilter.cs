using VTMap.Core.Enums;
using VTMap.Core.Interfaces;

namespace VTMap.Core.Filter
{
    public class TypeEqualsFilter : Filter
    {
        public GeometryType Type { get; }

        public TypeEqualsFilter(GeometryType type)
        {
            Type = type;
        }

        public override bool Evaluate(IVectorElement feature)
        {
            return feature != null && feature.Type.Equals(Type);
        }
    }
}
