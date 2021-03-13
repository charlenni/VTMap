using BruTile;
using SkiaSharp;
using VTMap.Common.Primitives;
using VTMap.Core.Primitives;

namespace VTMap.MapboxGL
{
    public class MGLPathSymbol : Symbol
    {
        public TileIndex TileIndex { get; private set; }

        public MGLPathSymbol(TileIndex tileIndex, string id)
        {
            TileIndex = tileIndex;
            Id = id;
        }

        public override void OnCalcBoundings()
        {
            //throw new NotImplementedException();
        }

        public override void OnDraw(SKCanvas canvas, EvaluationContext context)
        {
            //throw new NotImplementedException();
        }
    }
}
