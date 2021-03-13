using VTMap.Core.Enums;
using VTMap.Core.Primitives;

namespace VTMap.Core.Interfaces
{
    public interface IVectorElement
    {
        string Id { get; }

        GeometryType Type { get; }

        TagsCollection Tags { get; }
    }
}
