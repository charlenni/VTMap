using System;
using VTMap.Core.Primitives;

namespace VTMap.Core.Primitives
{
    public class Box
    {
        public float MinX = float.PositiveInfinity;
        public float MaxX = float.NegativeInfinity;
        public float MinY = float.PositiveInfinity;
        public float MaxY = float.NegativeInfinity;

        /// <summary>
        /// Create a new box with given min/max coordinates and check, if they are right
        /// </summary>
        /// <param name="x1">The x coordinate of first point</param>
        /// <param name="y1">The y coordinate of first point</param>
        /// <param name="x2">The x coordinate of second point</param>
        /// <param name="y2">The y coordinate of second point</param>
        /// <returns></returns>
        public static Box CreateSafe(float x1, float y1, float x2, float y2)
        {
            return new Box(x1 < x2 ? x1 : x2, y1 < y2 ? y1 : y2, x1 > x2 ? x1 : x2, y1 > y2 ? y1 : y2);
        }

        /// <summary>
        /// Instantiates a new Box with all values being 0
        /// </summary>
        public Box()
        {
        }

        /// <summary>
        /// Simple box instantiation (for adding extents)
        /// </summary>
        /// <param name="x">The initial x value</param>
        /// <param name="y">The initial y value</param>
        public Box(float x, float y)
        {
            MaxX = MinX = x;
            MaxY = MinY = y;
        }

        /// <summary>
        /// Instantiates a new Box
        /// </summary>
        /// <param name="MinX">The min x</param>
        /// <param name="MinY">The min y</param>
        /// <param name="MaxX">The max x</param>
        /// <param name="MaxY">The max x</param>
        public Box(float minX, float minY, float maxX, float maxY)
        {
            if (minX > maxX || minY > maxY)
                throw new ArgumentException("Minimum values must less than maximum values!");

            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public Box(Box box)
        {
            MinX = box.MinX;
            MinY = box.MinY;
            MaxX = box.MaxX;
            MaxY = box.MaxY;
        }

        /// <summary>
        /// Extend this box, so it includes new point
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public void Add(float x, float y)
        {
            if (x < MinX)
                MinX = x;
            if (y < MinY)
                MinY = y;
            if (x > MaxX)
                MaxX = x;
            if (y > MaxY)
                MaxY = y;
        }

        /// <summary>
        /// Extend this box about a given box
        /// </summary>
        /// <param name="box">Box to extend with</param>
        public void Add(Box box)
        {
            if (box.MinX < MinX)
                MinX = box.MinX;
            if (box.MinY < MinY)
                MinY = box.MinY;
            if (box.MaxX > MaxX)
                MaxX = box.MaxX;
            if (box.MaxY > MaxY)
                MaxY = box.MaxY;
        }

        /// <summary>
        /// Check, if Box contains point defined by coordinates x and y
        /// </summary>
        /// <param name="x">The x ordinate</param>
        /// <param name="y">The y ordinate</param>
        /// <returns>True, if point is inside box</returns>
        public bool Contains(float x, float y)
        {
            return (x >= MinX && x <= MaxX && y >= MinY && y <= MaxY);
        }

        /// <summary>
        /// Check if Box contains Point
        /// </summary>
        /// <param name="p">The point to check</param>
        public bool Contains(Point p)
        {
            return (p.X >= MinX && p.X <= MaxX && p.Y >= MinY && p.Y <= MaxY);
        }

        /// <summary>
        /// Height of this box
        /// </summary>
        public float Height
        {
            get
            {
                return MaxY - MinY;
            }
        }

        /// <summary>
        /// Width of this box
        /// </summary>
        public float Width
        {
            get
            {
                return MaxX - MinX;
            }
        }

        /// <summary>
        /// Check, if this Box is inside another box
        /// </summary>
        public bool Inside(Box box)
        {
            return MinX >= box.MinX && MaxX <= box.MaxX && MinY >= box.MinY && MaxY <= box.MaxY;
        }

        /// <summary>
        /// Check, if this box overlaps with another box
        /// </summary>
        /// <param name="other">The other box</param>
        /// <returns></returns>
        public bool Overlap(Box other)
        {
            return !(MinX > other.MaxX || MaxX < other.MinX || MinY > other.MaxY || MaxY < other.MinY);
        }

        /// <summary>
        /// Scale this box by a factor
        /// </summary>
        /// <param name="d">The factor to scale with</param>
        public void Scale(float d)
        {
            MinX *= d;
            MaxX *= d;
            MinY *= d;
            MaxY *= d;
        }

        /// <summary>
        /// Translate this box with a factor
        /// </summary>
        /// <param name="dx">The factor for translation in x direction</param>
        /// <param name="dy">The factor for translation in y direction</param>
        public void Translate(float dx, float dy)
        {
            MinX += dx;
            MaxX += dx;
            MinY += dy;
            MaxY += dy;
        }

        /// <summary>
        /// Expand this box about d in both directions
        /// </summary>
        /// <param name="d">Expand about d in both directions</param>
        public void Expand(float d)
        {
            MinX -= d;
            MaxX += d;
            MinY -= d;
            MaxY += d;
        }

        /// <summary>
        /// Expand this box about dx and dy
        /// </summary>
        /// <param name="dx">Expand about dx in x direction</param>
        /// <param name="dy">Expand about dy in y direction</param>
        public void Expand(float dx, float dy)
        {
            MinX -= dx;
            MaxX += dx;
            MinY -= dy;
            MaxY += dy;
        }

        /// <summary>
        /// Init or overwrite extents of box
        /// </summary>
        /// <param name="points">The points to extend to</param>
        public void SetExtents(float[] points)
        {
            SetExtents(points, points.Length);
        }

        /// <summary>
        /// Init or overwrite extents of box
        /// </summary>
        /// <param name="points">The points to extend to</param>
        /// <param name="length">The number of considered points</param>
        public void SetExtents(float[] points, int length)
        {
            float x1, y1, x2, y2;
            x1 = x2 = points[0];
            y1 = y2 = points[1];

            for (int i = 2; i < length; i += 2)
            {
                float x = points[i];
                if (x < x1)
                    x1 = x;
                else if (x > x2)
                    x2 = x;

                float y = points[i + 1];
                if (y < y1)
                    y1 = y;
                else if (y > y2)
                    y2 = y;
            }
            MinX = x1;
            MinY = y1;
            MaxX = x2;
            MaxY = y2;
        }

        public override string ToString()
        {
            return "[" + MinX + ',' + MinY + ',' + MaxX + ',' + MaxY + ']';
        }
    }
}
