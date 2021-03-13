using BruTile;
using SkiaSharp;
using VTMap.Common.Interfaces;
using VTMap.Core.Interfaces;
using VTMap.Core.Layers;
using VTMap.View;

namespace VectorTilesView.Layers
{
    public class BackgroundLayer : Layer, IRenderer
    {
        SKColor _color = SKColors.LightGray;
        SKPaint _paint = new SKPaint { IsAntialias = true, };
        IDrawableTileSource _source;

        public BackgroundLayer(SKColor color)
        {
            _color = color;
            _renderer = this;
        }

        public BackgroundLayer(IDrawableTileSource tileSource)
        {
            _source = tileSource;
            _renderer = this;
        }

        public SKColor Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    _paint.Color = _color;
                }
            }
        }

        public void Draw(object canvasObj, object viewportObj)
        {
            var canvas = (SKCanvas)canvasObj;
            var viewport = (Viewport)viewportObj;

            if (_source != null)
            {
                canvas.Save();

                foreach (var tile in viewport.Tiles.AsReadOnly())
                {
                    // Get matrix for tile
                    var matrix = viewport.MatrixForTile(tile);

                    canvas.SetMatrix(matrix);

                    var tileInfo = new TileInfo();
                    tileInfo.Index = tile;

                    canvas.DrawDrawable(_source.GetDrawableTile(tileInfo), 0, 0);
                }

                canvas.Restore();

                return;
            }

            canvas.Clear(_color);
        }
    }
}
