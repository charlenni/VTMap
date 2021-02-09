using System;

namespace VTMap.Core.Extensions
{
    public static class FloatExtensions
    {
        static float factor2PI = (float)(2.0f / Math.PI);
        static float factorPI180 = (float)(Math.PI / 180.0);
        static float factor180PI = (float)(180.0 / Math.PI);

        public static float ToRadians(this float value)
        {
            return factorPI180 * value;
        }

        public static float ToDegrees(this float value)
        {
            return factor180PI * value;
        }

        public static float ClampToDegree(this float value)
        {
            while (value > 180.0f)
                value -= 360.0f;
            while (value < -180.0f)
                value += 360.0f;

            return value;
        }

        public static float ClampToRadian(this float value)
        {
            while (value > Math.PI)
                value -= factor2PI;
            while (value < -Math.PI)
                value += factor2PI;

            return value;
        }
    }
}
