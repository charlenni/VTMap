namespace VTMap.Core.Interfaces
{
    public interface IProjection
    {
        /// <summary>
        /// Projects a latitude coordinate (in degrees) to the range [-1:+1]
        /// </summary>
        /// <param name="latitude">Latitude coordinate that should be converted</param>
        /// <returns>View position in range [-1:+1]</returns>
        float LongitudeToX(float longitude);

        /// <summary>
        /// Projects a longitude coordinate (in degrees) to the range [-1:+1]
        /// </summary>
        /// <param name="longitude">Longitude coordinate that should be converted</param>
        /// <returns>View position in range [-1:+1]</returns>
        float LatitudeToY(float latitude);

        /// <summary>
        /// Projects a value in range [-1:+1] to a longitude coordinate (in degrees)
        /// </summary>
        /// <param name="y">View position in range [-1:+1]</param>
        /// <returns>Longitude coordinate that should be converted</returns>
        float XToLongitude(float x);

        /// <summary>
        /// Projects a value in range [-1:+1] to a latitude coordinate (in degrees)
        /// </summary>
        /// <param name="x">View position in range [-1:+1]</param>
        /// <returns>Latitude coordinate that should be converted</returns>
        float YToLatitude(float y);
    }
}
