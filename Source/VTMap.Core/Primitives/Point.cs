using System.Diagnostics;
using System.Globalization;

namespace VTMap.Core
{
    [DebuggerDisplay("X={X}, Y={Y}")]
    public struct Point
    {
        public static Point Empty = new Point(0f, 0f, false);

        public Point(double x, double y)
        {
            _x = (float)x;
            _y = (float)y;
            _defined = true;
        }

        public Point(float x, float y)
        {
            _x = x;
            _y = y;
            _defined = true;
        }

        public Point(float x, float y, bool defined)
        {
            _x = x;
            _y = y;
            _defined = defined;
        }

        private float _x;
        private float _y;
        private bool _defined;

        public bool IsDefined { get => _defined; }

        public float X 
        { 
            get => _x;
            set 
            {
                _x = value;
                _defined = true;
            } 
        }
        
        public float Y
        { 
            get => _y;
            set 
            {
                _y = value;
                _defined = true;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Point other && other.IsDefined && _x == other.X && _y == other.Y;
        }

        public override int GetHashCode()
        {
            int result = 7;
            result = 31 * result + (int)(_x * 100000);
            result = 31 * result + (int)(_y * 100000);
            return result;
        }

        public Point Clone()
        {
            return new Point(_x, _y);
        }

        public override string ToString()
        {
            return $"X={_x.ToString(CultureInfo.InvariantCulture)}, Y={_y.ToString(CultureInfo.InvariantCulture)}";
        }

        public static Point operator +(Point first, Point second) => new Point(first.X + second.X, first.Y + second.Y);
        public static Point operator -(Point first, Point second) => new Point(first.X - second.X, first.Y - second.Y);
        public static Point operator *(Point first, float scalar) => new Point(first.X * scalar, first.Y * scalar);
        public static Point operator *(float scalar, Point first) => new Point(first.X * scalar, first.Y * scalar);
        public static Point operator /(Point first, float scalar) => new Point(first.X / scalar, first.Y / scalar);
        public static Point operator /(float scalar, Point first) => new Point(first.X / scalar, first.Y / scalar);
        public static bool operator ==(Point first, Point second) => first.IsDefined && !second.IsDefined && first.X == second.X && first.Y == second.Y;
        public static bool operator !=(Point first, Point second) => !(first == second);
    }
}
