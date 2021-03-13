using SkiaSharp;
using VTMap.Common.Primitives;
using VTMap.Core.Primitives;

namespace VTMap.MapboxGL
{
    public class MGLTextSymbol : Symbol
    {
        private SKRect envelope = SKRect.Empty;

        public override void OnCalcBoundings()
        {
        }

        public override void OnDraw(SKCanvas canvas, EvaluationContext context)
        {
            if (IsVisible)
            {

            }
        }
    }
}
