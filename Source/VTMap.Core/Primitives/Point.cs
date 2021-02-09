namespace VTMap.Core
{
    public class Point
    {
        public static Point Zero = new Point(0f, 0f);

        public Point(double x, double y)
        {
            X = (float)x;
            Y = (float)y;
        }

        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; set; }
        public float Y { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Point && X == ((Point)obj).X && Y == ((Point)obj).Y;
        }

        public override int GetHashCode()
        {
            int result = 7;
            result = 31 * result + (int)(X * 100000);
            result = 31 * result + (int)(Y * 100000);
            return result;
        }

        public Point Clone()
        {
            return new Point(X, Y);
        }

        public override string ToString()
        {
            return $"X={X}, Y={Y}";
        }

        public static Point operator +(Point first, Point second) => new Point(first.X + second.X, first.Y + second.Y);
        public static Point operator -(Point first, Point second) => new Point(first.X - second.X, first.Y - second.Y);
        public static Point operator *(Point first, float scalar) => new Point(first.X * scalar, first.Y * scalar);
        public static Point operator *(float scalar, Point first) => new Point(first.X * scalar, first.Y * scalar);
        public static Point operator /(Point first, float scalar) => new Point(first.X / scalar, first.Y / scalar);
        public static Point operator /(float scalar, Point first) => new Point(first.X / scalar, first.Y / scalar);
        public static bool operator ==(Point first, Point second) => first.X == second.X && first.Y == second.Y;
        public static bool operator !=(Point first, Point second) => first.X != second.X || first.Y != second.Y;
    }
}
