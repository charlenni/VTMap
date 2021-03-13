using SkiaSharp;
using VTMap.Core;
using VTMap.Core.Primitives;

namespace VTMap.Common.Extensions
{
    public static class SKPointExtension
    {
        public static Point ToPoint(this SKPoint point)
        {
            return new Point(point.X, point.Y);
        }

        public static SKPoint ToSKPoint(this Point point)
        {
            return new SKPoint(point.X, point.Y);
        }

        public static Point[] ToPoints(this SKPoint[] points)
        {
            Point[] result = new Point[points.Length];
            int i = 0;

            foreach (var point in points)
                result[i++] = point.ToPoint();

            return result;
        }

        public static SKPoint[] ToSKPoints(this Point[] points)
        {
            SKPoint[] result = new SKPoint[points.Length];
            int i = 0;

            foreach (var point in points)
                result[i++] = point.ToSKPoint();

            return result;
        }
    }
}
