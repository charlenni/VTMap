using VTMap.Core.Enums;
using VTMap.Core.Interfaces;

namespace VTMap.Core.Filter
{
    public class TypeNotEqualsFilter : Filter
    {
        public GeometryType Type { get; }

        public TypeNotEqualsFilter(GeometryType type)
        {
            Type = type;
        }

        public override bool Evaluate(IVectorElement feature)
        {
            return feature != null && !feature.Type.Equals(Type);
        }
    }
}
