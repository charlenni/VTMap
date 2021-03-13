using SkiaSharp;
using VTMap.Core.Primitives;

namespace VTMap.Common.Interfaces
{
    public interface IBucket
    {
        void OnDraw(SKCanvas canvas, EvaluationContext context);
    }
}
