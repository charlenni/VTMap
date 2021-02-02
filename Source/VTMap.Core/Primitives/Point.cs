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

        public static Point operator +(Point first, Point second) => new Point(first.X + second.X, first.Y + second.Y);
        public static Point operator -(Point first, Point second) => new Point(first.X - second.X, first.Y - second.Y);
        public static bool operator ==(Point first, Point second) => first.X == second.X && first.Y == second.Y;
        public static bool operator !=(Point first, Point second) => first.X != second.X || first.Y != second.Y;
    }
}
