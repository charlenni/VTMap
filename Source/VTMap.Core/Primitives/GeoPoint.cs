using System;
using VTMap.Core.Extensions;
using VTMap.Core.Projections;

namespace VTMap.Core
{
    /// <summary>
    /// A GeoPoint represents an immutable pair of latitude and longitude coordinates
    /// </summary>
    public class GeoPoint
    {
        /// <summary>
        /// Conversion factor from degrees to microdegrees
        /// </summary>
        public static readonly float ConversionFactor = 1000000f;

        /// <summary>
        /// The equatorial radius as defined by the <a href="http://en.wikipedia.org/wiki/World_Geodetic_System">WGS84
        /// ellipsoid</a>. WGS84 is the reference coordinate system used by the Global Positioning System.
        /// </summary>
        static readonly float EarthCircumference = 6378137.0f;

        /// <summary>
        /// The flattening factor of the earth's ellipsoid is required for distance computation
        /// </summary>
        static readonly float InverseFlattening = 298.257223563f;

        /// <summary>
        /// Polar radius of earth is required for distance computation
        /// </summary>
        static readonly float PolarRadius = 6356752.3142f;

        /// <summary>
        /// The hash code of this object
        /// </summary>
        int hashCodeValue = 0;

        /// <summary>
        /// The latitude value of this GeoPoint in degrees
        /// </summary>
        public float Latitude { get; }

        /// <summary>
        /// The longitude value of this GeoPoint in degrees
        /// </summary>
        public float Longitude { get; }

        /// <summary>
        /// Calculates the amount of degrees of latitude for a given distance in meters
        /// </summary>
        /// <param name="meters">Distance in meters</param>
        /// <returns>Distance in degrees</returns>
        public static float LatitudeDistance(int meters)
        {
            return (meters * 360f) / (float)(2 * Math.PI * EarthCircumference);
        }

        /// <summary>
        /// Calculates the amount of degrees of longitude for a given distance in meters
        /// </summary>
        /// <param name="meters">Distance in meters</param>
        /// <param name="latitude">Latitude in degrees</param>
        /// <returns>Distance in degrees</returns>
        public static float LongitudeDistance(int meters, float latitude)
        {
            return (meters * 360f) / (float)(2 * Math.PI * EarthCircumference * Math.Cos(latitude.ToRadians()));
        }

        /// <summary>
        /// Create a GeoPoint with given latitude and longitude
        /// </summary>
        /// <param name="lat">Latitude of new GeoPoint in degrees</param>
        /// <param name="lon">Longitude of new GeoPoint in degrees</param>
        public GeoPoint(float lat, float lon)
        {
            Latitude = Math.Max(Math.Min(lat, MercatorProjection.LatitudeMax), MercatorProjection.LatitudeMin);
            Longitude = Math.Max(Math.Min(lon, MercatorProjection.LongitudeMax), MercatorProjection.LongitudeMin);
        }

        /// <summary>
        /// Create a GeoPoint with given latitude and longitude in microdegrees
        /// </summary>
        /// <param name="lat">Latitude of new GeoPoint in microdegrees</param>
        /// <param name="lon">Longitude of new GeoPoint in microdegrees</param>
        public GeoPoint(int latitudeE6, int longitudeE6) : this(latitudeE6 / ConversionFactor, longitudeE6 / ConversionFactor)
        {
        }

        /// <summary>
        /// Bearing to another GeoPoint
        /// </summary>
        /// <param name="other">Other GeoPoint for calculation</param>
        /// <returns>Bearing from this GeoPoint to another GeoPoint</returns>
        public float BearingTo(GeoPoint other)
        {
            double deltaLon = (other.Longitude - Longitude).ToRadians();

            double a1 = Latitude.ToRadians();
            double b1 = other.Latitude.ToRadians();

            double y = Math.Sin(deltaLon) * Math.Cos(b1);
            double x = Math.Cos(a1) * Math.Sin(b1) - Math.Sin(a1) * Math.Cos(b1) * Math.Cos(deltaLon);
            double result = Math.Atan2(y, x).ToDegrees();

            return (float)((result + 360.0) % 360.0);
        }

        /// <summary>
        /// Compare this GeoPoint to another one
        /// </summary>
        /// <param name="other">Other GeoPoint to compore to</param>
        /// <returns>0, if both GeoPoints are equal</returns>
        public int CompareTo(GeoPoint other)
        {
            if (Equals(other))
            {
                return 0;
            }
            else if (Longitude > other.Longitude)
            {
                return 1;
            }
            else if (Longitude < other.Longitude)
            {
                return -1;
            }
            else if (Latitude > other.Latitude)
            {
                return 1;
            }
            else if (Latitude < other.Latitude)
            {
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// Returns the destination point from this point having travelled the given distance on the
        /// given initial bearing(bearing normally varies around path followed).
        /// See <a href="http://www.movable-type.co.uk/scripts/latlon.js">latlon.js</a>
        /// </summary>
        /// <param name="distance">Distance travelled, in same units as earth radius (default: meters)</param>
        /// <param name="bearing">Initial bearing in degrees from north</param>
        /// <returns>Destination point</returns>
        public GeoPoint DestinationPoint(float distance, float bearing)
        {
            double theta = bearing.ToRadians();
            double delta = distance / EarthCircumference; // angular distance in radians

            double phi1 = Latitude.ToRadians();
            double lambda1 = Longitude.ToRadians();

            double phi2 = Math.Asin(Math.Sin(phi1) * Math.Cos(delta)
                    + Math.Cos(phi1) * Math.Sin(delta) * Math.Cos(theta));
            double lambda2 = lambda1 + Math.Atan2(Math.Sin(theta) * Math.Sin(delta) * Math.Cos(phi1),
                    Math.Cos(delta) - Math.Sin(phi1) * Math.Sin(phi2));

            return new GeoPoint((float)phi2.ToDegrees(), (float)lambda2.ToDegrees());
        }

        /// <summary>
        /// Calculate the Euclidean distance from this point to another using the Pythagorean theorem
        /// </summary>
        /// <param name="other">Other GeoPoint to calculate the distance to</param>
        /// <returns>Distance in Degrees</returns>
        public float Distance(GeoPoint other)
        {
            return (float)MathExtensions.Hypot(Longitude - other.Longitude, Latitude - other.Latitude);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            else if (!(obj is GeoPoint)) {
                return false;
            }

            var other = obj as GeoPoint;

            if (Latitude - other.Latitude > float.Epsilon)
            {
                return false;
            }
            else if (Longitude - other.Longitude > float.Epsilon)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            if (hashCodeValue == 0)
                hashCodeValue = CalculateHashCode();

            return hashCodeValue;
        }

        public void Project(Point point)
        {
            point.X = MercatorProjection.LongitudeToX(Longitude);
            point.Y = MercatorProjection.LatitudeToY(Latitude);
        }

        /// <summary>
        /// Calculate the spherical distance from this point to another using the Haversine formula.
        /// <p>
        /// This calculation is done using the assumption, that the earth is a sphere, it is not
        /// though.If you need a higher precision and can afford a longer execution time you might
        /// want to use vincentyDistance.
        /// </p>
        /// </summary>
        /// <param name="other">>Other GeoPoint, to which the distance should be calculated</param>
        /// <returns>Distance between this GeoPoint and the other</returns>
        public float SphericalDistance(GeoPoint other)
        {
            double dLat = (other.Latitude - Latitude).ToRadians();
            double dLon = (other.Longitude - Longitude).ToRadians();
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(Latitude.ToRadians())
                     * Math.Cos(other.Latitude.ToRadians()) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return (float)(c * EarthCircumference);
        }

        /// <summary>
        /// Calculate the spherical distance from this point to another using Vincenty inverse formula
        /// for ellipsoids.This is very accurate but consumes more resources and time than the
        /// sphericalDistance method.
        /// <p>
        /// Adaptation of Chriss Veness' JavaScript Code on
        /// http://www.movable-type.co.uk/scripts/latlong-vincenty.html
        /// </p>
        /// <p>
        /// Paper: Vincenty inverse formula - T Vincenty, "Direct and Inverse Solutions of Geodesics
        /// on the Ellipsoid with application of nested equations", Survey Review, vol XXII no 176,
        /// 1975 (http://www.ngs.noaa.gov/PUBS_LIB/inverse.pdf)
        /// </p>
        /// </summary>
        /// <param name="other">Other GeoPoint, to which the distance should be calculated</param>
        /// <returns>Distance between this GeoPoint and the other</returns>
        public float VincentyDistance(GeoPoint other)
        {
            double f = 1 / InverseFlattening;
            double L = (other.Longitude - Longitude).ToRadians();
            double U1 = Math.Atan((1 - f) * Math.Tan(Latitude.ToRadians()));
            double U2 = Math.Atan((1 - f) * Math.Tan(other.Latitude.ToRadians()));
            double sinU1 = Math.Sin(U1), cosU1 = Math.Cos(U1);
            double sinU2 = Math.Sin(U2), cosU2 = Math.Cos(U2);

            double lambda = L, lambdaP, iterLimit = 100;

            double cosSqAlpha = 0, sinSigma = 0, cosSigma = 0, cos2SigmaM = 0, sigma = 0, sinLambda = 0, sinAlpha = 0, cosLambda = 0;
            do
            {
                sinLambda = Math.Sin(lambda);
                cosLambda = Math.Cos(lambda);
                sinSigma = Math.Sqrt((cosU2 * sinLambda) * (cosU2 * sinLambda)
                        + (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda)
                        * (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda));
                if (sinSigma == 0)
                    return 0; // co-incident points
                cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
                sigma = Math.Atan2(sinSigma, cosSigma);
                sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
                cosSqAlpha = 1 - sinAlpha * sinAlpha;
                if (cosSqAlpha != 0)
                {
                    cos2SigmaM = cosSigma - 2 * sinU1 * sinU2 / cosSqAlpha;
                }
                else
                {
                    cos2SigmaM = 0;
                }
                double C = f / 16 * cosSqAlpha * (4 + f * (4 - 3 * cosSqAlpha));
                lambdaP = lambda;
                lambda = L
                        + (1 - C)
                        * f
                        * sinAlpha
                        * (sigma + C * sinSigma
                        * (cos2SigmaM + C * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));
            } while (Math.Abs(lambda - lambdaP) > 1e-12 && --iterLimit > 0);

            if (iterLimit == 0)
                return 0; // formula failed to converge

            double uSq = cosSqAlpha
                    * (Math.Pow(EarthCircumference, 2) - Math.Pow(PolarRadius, 2))
                    / Math.Pow(PolarRadius, 2);
            double A = 1 + uSq / 16384 * (4096 + uSq * (-768 + uSq * (320 - 175 * uSq)));
            double B = uSq / 1024 * (256 + uSq * (-128 + uSq * (74 - 47 * uSq)));
            double deltaSigma = B
                    * sinSigma
                    * (cos2SigmaM + B
                    / 4
                    * (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM) - B / 6 * cos2SigmaM
                    * (-3 + 4 * sinSigma * sinSigma)
                    * (-3 + 4 * cos2SigmaM * cos2SigmaM)));
            double s = PolarRadius * A * (sigma - deltaSigma);

            return (float)s;
        }

        public override string ToString()
        {
            return $"Lat={Latitude}/Lon={Longitude}";
        }

        /// <summary>
        /// Calculate hash code for this GeoPoint
        /// </summary>
        /// <returns>Hash code</returns>
        int CalculateHashCode()
        {
            int result = 7;
            result = 31 * result + (int)(Latitude * ConversionFactor);
            result = 31 * result + (int)(Longitude * ConversionFactor);
            return result;
        }
    }
}
