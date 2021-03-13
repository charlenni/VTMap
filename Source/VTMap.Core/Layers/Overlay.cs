using System;
using VTMap.Core.Enums;
using VTMap.Core.Primitives;

namespace VTMap.Core.Layers
{
    /// <summary>
    /// An Overlay is a special form of layer, which stays always on top of the map
    /// </summary>
    /// <remarks>
    /// Normally it would be used for things, that don't move with the map like 
    /// scalebars, remarks or buttons.
    /// </remarks>
    public class Overlay : Layer
    {
        /// <summary>
        /// Horizontal position for the overlay, where to draw
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Vertical position for the overlay, where to draw
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// Margin in x direction for the overlay
        /// </summary>
        public float MarginX { get; set; }

        /// <summary>
        /// Margin in y direction for the overlay
        /// </summary>
        public float MarginY { get; set; }

        /// <summary>
        /// Box around the overlay content
        /// </summary>
        public Box BoundingBox { get; set; }

        public float CalculatePositionX(float left, float right, float width)
        {
            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    return MarginX;

                case HorizontalAlignment.Center:
                    return (right - left - width) / 2;

                case HorizontalAlignment.Right:
                    return right - left - width - MarginX;
            }

            throw new ArgumentException("Unknown horizontal alignment: " + HorizontalAlignment);
        }

        public float CalculatePositionY(float top, float bottom, float height)
        {
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    return MarginY;

                case VerticalAlignment.Bottom:
                    return bottom - top - height - MarginY;

                case VerticalAlignment.Center:
                    return (bottom - top - height) / 2;
            }

            throw new ArgumentException("Unknown vertical alignment: " + VerticalAlignment);
        }
    }
}
