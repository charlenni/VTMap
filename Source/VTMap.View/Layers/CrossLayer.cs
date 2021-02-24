using SkiaSharp;
using System.ComponentModel;
using VTMap.Core.Interfaces;
using VTMap.Core.Layers;

namespace VTMap.View.Layers
{
    public class CrossLayer : Layer, IRenderer, INotifyPropertyChanged
    {
        public CrossLayer()
        {
            _renderer = this;    
        }

        public SKColor Color { get; set; } = SKColors.Blue;

        public float Width { get; set; } = 50;

        public float Height { get; set; } = 50;

        public float StrokeWidth { get; set; } = 1;

        public void Draw(object canvasObj, object viewportObj)
        {
            var canvas = (SKCanvas)canvasObj;
            var viewport = (Viewport)viewportObj;
            var centerX = viewport.Width / 2;
            var centerY = viewport.Height / 2;

            var paintCross = new SKPaint { Color = Color, Style = SKPaintStyle.Stroke, StrokeWidth = StrokeWidth, };

            canvas.DrawLine(centerX - Width, centerY, centerX + Width, centerY, paintCross);
            canvas.DrawLine(centerX, centerY - Height, centerX, centerY + Height, paintCross);
        }
    }
}
