using System;

namespace VTMap.Core.Extensions
{
    public static class DoubleExtensions
    {
        static double factorPI180 = Math.PI / 180;
        static double factor180PI = 180 / Math.PI;

        public static double ToRadians(this double value)
        {
            return factorPI180 * value;
        }

        public static double ToDegrees(this double value)
        {
            return factor180PI * value;
        }

        public static double ClampToDegree(this double value)
        {
            while (value > 180.0f)
                value -= 360.0f;
            while (value < 180.0f)
                value += 360.0f;

            return value;
        }

        public static double ClampToRadian(this double value)
        {
            while (value > Math.PI)
                value -= 2.0 * Math.PI;
            while (value < -Math.PI)
                value += 2.0 * Math.PI;

            return value;
        }

        public static bool IsNanOrInfOrZero(this double target)
        {
            if (double.IsNaN(target)) return true;
            if (double.IsInfinity(target)) return true;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return target == 0;
        }
    }
}
