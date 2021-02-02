using System;

namespace VTMap.Core.Projections
{
    public class MercatorProjection
    {
        // The circumference of the earth at the equator in meters
        public static readonly float EarthCircumference = 40075016.686f;

        // Maximum possible latitude coordinate of the map
        public static readonly float LatitudeMax = 85.05112877980659f;

        // Minimum possible latitude coordinate of the map.
        public static readonly float LatitudeMin = -LatitudeMax;

        // Maximum possible longitude coordinate of the map
        public static readonly float LongitudeMax = 180.0f;

        // Minimum possible longitude coordinate of the map
        public static readonly float LongitudeMin = -LongitudeMax;

        /// <summary>
        /// Projects a latitude coordinate (in degrees) to the range [0.0,1.0]
        /// </summary>
        /// <param name="latitude">Latitude coordinate that should be converted</param>
        /// <returns>View position in range [0.0,1.0]</returns>
        public static float LongitudeToX(float longitude)
        {
            return (longitude + 180.0f) / 360.0f;
        }

        /// <summary>
        /// Projects a longitude coordinate (in degrees) to the range [0.0,1.0]
        /// </summary>
        /// <param name="longitude">Longitude coordinate that should be converted</param>
        /// <returns>View position in range [0.0,1.0]</returns>
        public static float LatitudeToY(float latitude)
        {
            double sinLatitude = Math.Sin(latitude * (Math.PI / 180));

            return (float)Math.Max(Math.Min(0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI), 1.0), 0.0);
        }

        /// <summary>
        /// Limit latitude to min and max values
        /// </summary>
        /// <param name="latitude">Latitude value which should be limited</param>
        /// <returns>Given latitude value, limited to the possible latitude range</returns>
        public static float LimitLatitude(float latitude)
        {
            return (float)Math.Max(Math.Min(latitude, LatitudeMax), LatitudeMin);
        }

        /// <summary>
        /// Limit longitude to min and max values
        /// </summary>
        /// <param name="longitude">Longitude value which should be limited</param>
        /// <returns>Given longitude value, limited to the possible longitude range</returns>
        public static float LimitLongitude(float longitude)
        {
            return (float)Math.Max(Math.Min(longitude, LongitudeMax), LongitudeMin);
        }

        /// <summary>
        /// Converts y view position to latitude in degrees
        /// </summary>
        /// <param name="y">y the view position <see cref="ViewPosition.Y"/></param>
        /// <returns>Latitude in degrees</returns>
        public static float ToLatitude(float y)
        {
            return 90f - 360f * (float)(Math.Atan(Math.Exp((y - 0.5) * (2 * Math.PI))) / Math.PI);
        }

        /// <summary>
        /// Converts x view position to latitude in degrees
        /// </summary>
        /// <param name="x">x the view position <see cref="ViewPosition.X"/></param>
        /// <returns>Longitude in degrees</returns>
        public static float ToLongitude(float x)
        {
            return 360.0f * (x - 0.5f);
        }
    }
}
