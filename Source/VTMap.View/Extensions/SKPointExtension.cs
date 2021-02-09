using SkiaSharp;
using VTMap.Core;

namespace VTMap.Extensions
{
    public static class SKPointExtension
    {
        public static Point ToPoint(this SKPoint point)
        {
            return new Point(point.X, point.Y);
        }
    }
}
