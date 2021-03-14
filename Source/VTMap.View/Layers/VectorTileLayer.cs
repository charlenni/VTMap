using BruTile;
using SkiaSharp;
using System.Collections.Generic;
using VTMap.Common.Interfaces;
using VTMap.Common.Primitives;
using VTMap.Core.Interfaces;
using VTMap.Core.Layers;
using VTMap.View;

namespace VectorTilesView.Layers
{
    public class VectorTileLayer : Layer, IRenderer
    {
        IDrawableTileSource _source;
        Dictionary<TileIndex, Drawable> _cache = new Dictionary<TileIndex, Drawable>();

        public VectorTileLayer(IDrawableTileSource tileSource)
        {
            _source = tileSource;
            _renderer = this;
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
                    tileInfo.Index = new TileIndex(tile.Col, _source.Schema.YAxis == YAxis.TMS ? (1 << tile.Level) - tile.Row - 1 : tile.Row, tile.Level); ;

                    if (!_cache.ContainsKey(tileInfo.Index))
                        _cache.Add(tileInfo.Index, _source.GetDrawableTile(tileInfo));

                    var drawable = _cache[tileInfo.Index];

                    drawable.Context.Zoom = viewport.ZoomLevel;
                    drawable.Context.Scale = 1;

                    if (drawable != null)
                    {
                        //canvas.ClipRect(drawable.Bounds);
                        canvas.DrawDrawable(drawable, 0, 0);
                    }
                }

                canvas.Restore();

                return;
            }
        }
    }
}
