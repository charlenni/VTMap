using SkiaSharp;
using Svg.Skia;
using System;
using VTMap.Core.Interfaces;
using VTMap.Core.Layers;
using VTMap.Core.Primitives;

namespace VTMap.View.Layers
{
    public class DemoLayer : Layer, IRenderer
    {
        const int pinNum = 1000;
        Point[] points = new Point[pinNum];
        SKPicture[] pins = new SKPicture[pinNum];
        float[] scales = new float[pinNum];
        Random rand = new Random();

        public DemoLayer()
        {
            _renderer = this;

            for (var i = 0; i < pinNum; i++)
            {
                points[i] = new Point(-1 + rand.NextDouble() * 2, -1 + rand.NextDouble() * 2);
                scales[i] = 0.5f + (float)rand.NextDouble();

                var svg = new SKSvg();
                var colorInHex = $"{rand.Next(0, 255):X2}{rand.Next(0, 255):X2}{rand.Next(0, 255):X2}";
                svg.FromSvg($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"36\" height=\"56\"><path d=\"M18 .34C8.325.34.5 8.168.5 17.81c0 3.339.962 6.441 2.594 9.094H3l7.82 15.117L18 55.903l7.187-13.895L33 26.903h-.063c1.632-2.653 2.594-5.755 2.594-9.094C35.531 8.169 27.675.34 18 .34zm0 9.438a6.5 6.5 0 1 1 0 13 6.5 6.5 0 0 1 0-13z\" fill=\"#{colorInHex}\"/></svg>");
                pins[i] = svg.Picture;
            }
        }

        public void Draw(object canvasObj, object viewportObj)
        {
            var canvas = (SKCanvas)canvasObj;
            var viewport = (Viewport)viewportObj;
            var paint = new SKPaint { Color = SKColors.Red, IsStroke = false, TextSize = Viewport.TileSize / 16, TextAlign = SKTextAlign.Center, };

            Point point;
            SKPoint skpoint = new SKPoint();

            for (var i = 0; i < pinNum; i++)
            {
                point = viewport.FromViewToScreen(points[i]);
             
                skpoint.X = point.X - (pins[i].CullRect.Width * scales[i]) / 2;
                skpoint.Y = point.Y - (pins[i].CullRect.Height * scales[i]);

                canvas.Save();
                canvas.Translate(skpoint.X, skpoint.Y);
                canvas.Scale(scales[i]);
                canvas.Translate(-skpoint.X, -skpoint.Y);
                canvas.DrawPicture(pins[i], skpoint.X, skpoint.Y, paint);
                canvas.Restore();
            }
        }
    }
}
