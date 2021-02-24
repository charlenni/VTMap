using SkiaSharp;
using System.ComponentModel;
using VTMap.Core.Interfaces;
using VTMap.Core.Layers;

namespace VTMap.View.Layers
{
    public class TileIdLayer : Layer, IRenderer, INotifyPropertyChanged
    {
        public TileIdLayer()
        {
            _renderer = this;    
        }

        public SKColor BorderColor { get; set; } = SKColors.Red;

        public SKColor TextColor { get; set; } = SKColors.Green;

        public float StrokeWidth { get; set; } = 1;

        public void Draw(object canvasObj, object viewportObj)
        {
            var canvas = (SKCanvas)canvasObj;
            var viewport = (Viewport)viewportObj;
            var tileCenter = Viewport.TileSize / 2;

            var tiles = viewport.Tiles.AsReadOnly();

            var paintBorder = new SKPaint { Color = BorderColor, Style = SKPaintStyle.Stroke, StrokeWidth = StrokeWidth, };
            var paintText = new SKPaint { Color = TextColor, IsStroke = false, TextSize = Viewport.TileSize / 16, TextAlign = SKTextAlign.Center, };

            var points = new SKPoint[4];

            points[0] = new SKPoint(0, 0);
            points[1] = new SKPoint(Viewport.TileSize, 0);
            points[2] = new SKPoint(Viewport.TileSize, Viewport.TileSize);
            points[3] = new SKPoint(0, Viewport.TileSize);

            var path = new SKPath();
            path.AddPoly(points, true);

            canvas.Save();

            foreach (var tile in tiles)
            {
                // Get matrix for tile
                var matrix = viewport.MatrixForTile(tile);

                canvas.SetMatrix(matrix);

                canvas.DrawPath(path, paintBorder);
                canvas.DrawCircle(Viewport.TileSize / 8, Viewport.TileSize / 8, Viewport.TileSize / 16, paintBorder);

                var text = $"Tile {tile.Col}/{tile.Row}/{tile.Level}";
                var width = paintText.MeasureText(text);

                canvas.DrawText(text, width / 2, Viewport.TileSize / 16, paintText);
                canvas.DrawText(text, width / 2, Viewport.TileSize - Viewport.TileSize / 64, paintText);
                canvas.DrawText(text, Viewport.TileSize - width / 2, Viewport.TileSize / 16, paintText);
                canvas.DrawText(text, Viewport.TileSize - width / 2, Viewport.TileSize - Viewport.TileSize / 64, paintText);

                // Draw text always horizontical
                canvas.SetMatrix(matrix.PreConcat(SKMatrix.CreateRotationDegrees(viewport.Rotation, tileCenter, tileCenter)));
                canvas.DrawText(text, tileCenter, tileCenter, paintText);
            }

            canvas.Restore();
        }
    }
}
