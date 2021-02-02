using System;

namespace VTMap.Core.Extensions
{
    public static class MathExtensions
    {
        /// <summary>
        /// Integer version of log2(x)
        /// </summary>
        /// <remarks>
        /// from http://graphics.stanford.edu/~seander/bithacks.html#IntegerLog
        /// </remarks>
        public static int Log2(int value)
        {
            int result = 0;

            if ((value & 0xFFFF0000) != 0)
            {
                value >>= 16;
                result |= 16;
            }
            if ((value & 0xFF00) != 0)
            {
                value >>= 8;
                result |= 8;
            }
            if ((value & 0xF0) != 0)
            {
                value >>= 4;
                result |= 4;
            }
            if ((value & 0xC) != 0)
            {
                value >>= 2;
                result |= 2;
            }
            if ((value & 0x2) != 0)
            {
                result |= 1;
            }
            return result;
        }

        public static double Hypot(double a, double b)
        {
            return Math.Sqrt(a * a + b * b);
        }
    }
}
