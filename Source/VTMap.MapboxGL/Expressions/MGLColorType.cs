using SkiaSharp;
using VTMap.MapboxGL.Extensions;

namespace VTMap.MapboxGL.Expressions
{
    internal class MGLColorType : MGLType
    {
        public MGLColorType(string v)
        {
            Value = v.FromString();
        }

        public MGLColorType(SKColor v)
        {
            Value = v;
        }

        public SKColor Value { get; }

        public override string ToString()
        {
            return "color";
        }
    }
}
