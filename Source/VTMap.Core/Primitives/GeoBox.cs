using System;
using System.Collections.Generic;
using System.Text;
using VTMap.Core.Projections;

namespace VTMap.Core
{
    /// <summary>
    /// A BoundingBox represents an immutable set of two latitude and two longitude coordinates
    /// </summary>
    public class GeoBox
    {
        /// <summary>
        /// Conversion factor from degrees to microdegrees
        /// </summary>
        static readonly double ConversionFactor = 1000000d;

        /// <summary>
        /// Maximum latitude from BoundingBox in degrees
        /// </summary>
        public float MaxLatitude { get; set; }

        /// <summary>
        /// Maximum longitude from BoundingBox in degrees
        /// </summary>
        public float MaxLongitude { get; set; }

        /// <summary>
        /// Minimum latitude from BoundingBox in degrees
        /// </summary>
        public float MinLatitude { get; set; }

        /// <summary>
        /// Minimum longitude from BoundingBox in degrees
        /// </summary>
        public float MinLongitude { get; set; }

        /// <summary>
        /// Creating a BoundingBox with degree values
        /// </summary>
        /// <param name="minLatitude">Minimum latitude in degrees</param>
        /// <param name="minLongitude">Minimum longitude in degrees</param>
        /// <param name="maxLatitude">Maximum latitude in degrees</param>
        /// <param name="maxLongitude">Maximum longitude in degrees</param>
        public GeoBox(float minLatitude, float minLongitude, float maxLatitude, float maxLongitude)
        {
            MinLatitude = minLatitude;
            MinLongitude = minLongitude;
            MaxLatitude = maxLatitude;
            MaxLongitude = maxLongitude;
        }

        /// <summary>
        /// Create a BoundingBox for a list of given GeoPoints
        /// </summary>
        /// <param name="geoPoints">List of GeoPoints</param>
        public GeoBox(List<GeoPoint> geoPoints)
        {
            float minLat = float.MaxValue;
            float minLon = float.MaxValue;
            float maxLat = float.MaxValue;
            float maxLon = float.MaxValue;

            foreach (GeoPoint geoPoint in geoPoints)
            {
                minLat = Math.Min(minLat, geoPoint.Latitude);
                minLon = Math.Min(minLon, geoPoint.Longitude);
                maxLat = Math.Max(maxLat, geoPoint.Latitude);
                maxLon = Math.Max(maxLon, geoPoint.Longitude);
            }

            MinLatitude = minLat;
            MinLongitude = minLon;
            MaxLatitude = maxLat;
            MaxLongitude = maxLon;
        }

        /// <summary>
        /// Checks, if the given GeoPoint is inside the BoundingBox
        /// </summary>
        /// <param name="geoPoint">GeoPoint to check</param>
        /// <returns>True, if this BoundingBox contains the given GeoPoint, false otherwise
        public bool Contains(GeoPoint geoPoint)
        {
            return geoPoint.Latitude <= MaxLatitude
                && geoPoint.Latitude >= MinLatitude
                && geoPoint.Longitude <= MaxLongitude
                && geoPoint.Longitude >= MinLongitude;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            else if (!(obj is GeoBox))
            {
                return false;
            }

            GeoBox other = obj as GeoBox;

            if (MaxLatitude != other.MaxLatitude)
            {
                return false;
            }
            else if (MaxLongitude != other.MaxLongitude)
            {
                return false;
            }
            else if (MinLatitude != other.MinLatitude)
            {
                return false;
            }
            else if (MinLongitude != other.MinLongitude)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Extend BoundingBox by another BoundingBox
        /// </summary>
        /// <param name="other">BoundingBox that should extend this BoundingBox</param>
        /// <returns>BoundingBox that covers this and the given BoundingBoxes</returns>
        public GeoBox ExtendBoundingBox(GeoBox other)
        {
            return new GeoBox(Math.Min(MinLatitude, other.MinLatitude),
                    Math.Min(MinLongitude, other.MinLongitude),
                    Math.Max(MaxLatitude, other.MaxLatitude),
                    Math.Max(MaxLongitude, other.MaxLongitude));
        }

        /// <summary>
        /// Creates a BoundingBox extended up to <code>GeoPoint</code> (but does not cross date line/poles)
        /// </summary>
        /// <param name="geoPoint">Coordinates up to the extension</param>
        /// <returns>An extended BoundingBox or this (if contains coordinates)</returns>
        public GeoBox ExtendCoordinates(GeoPoint geoPoint)
        {
            if (Contains(geoPoint))
            {
                return this;
            }

            float minLat = Math.Max(MercatorProjection.LatitudeMin, Math.Min(MinLatitude, geoPoint.Latitude));
            float minLon = Math.Max(MercatorProjection.LongitudeMin, Math.Min(MinLongitude, geoPoint.Longitude));
            float maxLat = Math.Min(MercatorProjection.LatitudeMax, Math.Max(MaxLatitude, geoPoint.Latitude));
            float maxLon = Math.Min(MercatorProjection.LongitudeMax, Math.Max(MaxLongitude, geoPoint.Longitude));

            return new GeoBox(minLat, minLon, maxLat, maxLon);
        }

        /// <summary>
        /// Creates a BoundingBox that is a fixed degree amount larger on all sides (but does not cross date line/poles)
        /// </ summary >
        /// <param name="verticalExpansion">Degree extension (must be >= 0)</param>
        /// <param name="horizontalExpansion">Degree extension (must be >= 0)</param>
        /// <returns>An extended BoundingBox or this (if degrees == 0)</returns>
        public GeoBox ExtendDegrees(float verticalExpansion, float horizontalExpansion)
        {
            if (verticalExpansion == 0 && horizontalExpansion == 0)
            {
                return this;
            }
            else if (verticalExpansion < 0 || horizontalExpansion < 0)
            {
                throw new ArgumentException("BoundingBox extend operation does not accept negative values");
            }

            float minLat = Math.Max(MercatorProjection.LatitudeMin, MinLatitude - verticalExpansion);
            float minLon = Math.Max(MercatorProjection.LongitudeMin, MinLongitude - horizontalExpansion);
            float maxLat = Math.Min(MercatorProjection.LatitudeMax, MaxLatitude + verticalExpansion);
            float maxLon = Math.Min(MercatorProjection.LongitudeMax, MaxLongitude + horizontalExpansion);

            return new GeoBox(minLat, minLon, maxLat, maxLon);
        }

        /// <summary>
        /// Creates a BoundingBox that is a fixed margin factor larger on all sides (but does not cross date line/poles)
        /// </summary>
        /// <param name="margin">Extension (must be > 0)</param>
        /// <returns>An extended BoundingBox or this (if margin == 1)</returns>
        public GeoBox ExtendMargin(float margin)
        {
            if (margin == 1)
            {
                return this;
            }
            else if (margin <= 0)
            {
                throw new ArgumentException("BoundingBox extend operation does not accept negative or zero values");
            }

            float verticalExpansion = (LatitudeSpan * margin - LatitudeSpan) * 0.5f;
            float horizontalExpansion = (LongitudeSpan * margin - LongitudeSpan) * 0.5f;

            float minLat = Math.Max(MercatorProjection.LatitudeMin, MinLatitude - verticalExpansion);
            float minLon = Math.Max(MercatorProjection.LongitudeMin, MinLongitude - horizontalExpansion);
            float maxLat = Math.Min(MercatorProjection.LatitudeMax, MaxLatitude + verticalExpansion);
            float maxLon = Math.Min(MercatorProjection.LongitudeMax, MaxLongitude + horizontalExpansion);

            return new GeoBox(minLat, minLon, maxLat, maxLon);
        }

        /// <summary>
        /// Creates a BoundingBox that is a fixed meter amount larger on all sides (but does not cross date line/poles)
        /// </summary>
        /// <param name="meters">Extension (must be >= 0)</param>
        /// <returns>An extended BoundingBox or this (if meters == 0)</returns>
        public GeoBox ExtendMeters(int meters)
        {
            if (meters == 0)
            {
                return this;
            }
            else if (meters < 0)
            {
                throw new ArgumentException("BoundingBox extend operation does not accept negative values");
            }

            float verticalExpansion = GeoPoint.LatitudeDistance(meters);
            float horizontalExpansion = GeoPoint.LongitudeDistance(meters, Math.Max(Math.Abs(MinLatitude), Math.Abs(MaxLatitude)));

            float minLat = Math.Max(MercatorProjection.LatitudeMin, MinLatitude - verticalExpansion);
            float minLon = Math.Max(MercatorProjection.LongitudeMin, MinLongitude - horizontalExpansion);
            float maxLat = Math.Min(MercatorProjection.LatitudeMax, MaxLatitude + verticalExpansion);
            float maxLon = Math.Min(MercatorProjection.LongitudeMax, MaxLongitude + horizontalExpansion);

            return new GeoBox(minLat, minLon, maxLat, maxLon);
        }

        public string Format()
        {
            return new StringBuilder()
                    .Append(MinLatitude)
                    .Append(',')
                    .Append(MinLongitude)
                    .Append(',')
                    .Append(MaxLatitude)
                    .Append(',')
                    .Append(MaxLongitude)
                    .ToString();
        }

        /// <summary>
        /// The GeoPoint at the horizontal and vertical center of this BoundingBox
        /// </summary>
        public GeoPoint CenterPoint
        {
            get
            {
                float latitudeOffset = (MaxLatitude - MinLatitude) / 2f;
                float longitudeOffset = (MaxLongitude - MinLongitude) / 2f;
                return new GeoPoint(MinLatitude + latitudeOffset, MinLongitude + longitudeOffset);
            }
        }

        /// <summary>
        /// The latitude span of this BoundingBox in degrees
        /// </summary>
        public float LatitudeSpan
        {
            get
            {
                return MaxLatitude - MinLatitude;
            }
        }

        /// <summary>
        /// The longitude span of this BoundingBox in degrees
        /// </summary>
        public float LongitudeSpan
        {
            get
            {
                return MaxLongitude - MinLongitude;
            }
        }

        /// <summary>
        /// The maximum latitude value of this BoundingBox in microdegrees
        /// </summary>
        public int MaxLatitudeE6
        {
            get
            {
                return (int)(MaxLatitude * ConversionFactor);
            }
        }

        /// <summary>
        /// The maximum longitude value of this BoundingBox in microdegrees
        /// </summary>
        public int MaxLongitudeE6
        {
            get 
            { 
                return (int)(MaxLongitude * ConversionFactor);
            }
        }

        /// <summary>
        /// The minimum latitude value of this BoundingBox in microdegrees
        /// </summary>
        public int MinLatitudeE6
        {
            get
            {
                return (int)(MinLatitude * ConversionFactor);
            }
        }

        /// <summary>
        /// The minimum longitude value of this BoundingBox in microdegrees
        /// </summary>
        public int MinLongitudeE6
        {
            get
            {
                return (int)(MinLongitude * ConversionFactor);
            }
        }

        public override int GetHashCode()
        {
            int result = 7;
            result = 31 * result + MaxLatitudeE6;
            result = 31 * result + MaxLongitudeE6;
            result = 31 * result + MinLatitudeE6;
            result = 31 * result + MinLongitudeE6;
            return result;
        }

        /// <summary>
        /// Check, if another BoundingBox intersects with this BoundingBox
        /// </summary>
        /// <param name="boundingBox">The BoundingBox which should be checked for intersection with this BoundingBox</param>
        /// <returns>True, if this BoundingBox intersects with the given BoundingBox, false otherwise</returns>
        public bool Intersects(GeoBox boundingBox)
        {
            if (this == boundingBox)
            {
                return true;
            }

            return MaxLatitude >= boundingBox.MinLatitude && MaxLongitude >= boundingBox.MinLongitude
                    && MinLatitude <= boundingBox.MaxLatitude && MinLongitude <= boundingBox.MaxLongitude;
        }

        /// <summary>
        /// Returns if an area built from the geoPoints intersects with a bias towards
        /// returning true
        /// The method returns fast if any of the points lie within the BoundingBox. If none of the points
        /// lie inside the box, it constructs the outer BoundingBox for all the points and tests for intersection
        /// (so it is possible that the area defined by the points does not actually intersect)
        /// </summary>
        /// <param name="geoPoints">The points that define an area</param>
        /// <returns>False if there is no intersection, true if there could be an intersection</returns>
        public bool IntersectsArea(GeoPoint[][] geoPoints)
        {
            if (geoPoints.Length == 0 || geoPoints[0].Length == 0)
            {
                return false;
            }
            foreach (GeoPoint[] outer in geoPoints)
            {
                foreach (GeoPoint geoPoint in outer)
                {
                    if (this.Contains(geoPoint))
                    {
                        // if any of the points is inside the bbox return early
                        return true;
                    }
                }
            }

            // no fast solution, so accumulate boundary points
            float tmpMinLat = geoPoints[0][0].Latitude;
            float tmpMinLon = geoPoints[0][0].Longitude;
            float tmpMaxLat = geoPoints[0][0].Latitude;
            float tmpMaxLon = geoPoints[0][0].Longitude;

            foreach (GeoPoint[] outer in geoPoints)
            {
                foreach (GeoPoint geoPoint in outer)
                {
                    tmpMinLat = Math.Min(tmpMinLat, geoPoint.Latitude);
                    tmpMaxLat = Math.Max(tmpMaxLat, geoPoint.Latitude);
                    tmpMinLon = Math.Min(tmpMinLon, geoPoint.Longitude);
                    tmpMaxLon = Math.Max(tmpMaxLon, geoPoint.Longitude);
                }
            }

            return this.Intersects(new GeoBox(tmpMinLat, tmpMinLon, tmpMaxLat, tmpMaxLon));
        }

        public override string ToString()
        {
            return $"BoundingBox [{MinLatitude},{MinLongitude} {MaxLatitude},{MaxLongitude}]";
        }
    }
}
