using System;
using VTMap.Core.Extensions;
using VTMap.Core.Projections;

namespace VTMap.Core.Map
{
    public class Camera
    {
        int zoomLevel;
        float bearing;
        float roll;
        float scale;

        // Projected position X 0..1
        public float X { get; set; }

        // Projected position Y 0..1
        public float Y { get; set; }

        // Absolute scale
        public float Scale 
        {
            get
            {
                return scale;
            }
            set 
            {
                if (scale != value)
                {
                    scale = value;
                    zoomLevel = MathExtensions.Log2((int)scale);
                }
            }
        }

        //Rotation angle
        public float Bearing
        {
            get
            { 
                return bearing; 
            }
            set
            {
                if (bearing != value)
                {
                    bearing = value.ClampToDegree();
                }
            }
        }

        // Perspective tilt
        public float Tilt { get; set; }

        // Perspective roll
        public float Roll
        {
            get
            {
                return roll;
            }
            set
            {
                if (roll != value)
                {
                    roll = value.ClampToDegree();
                }
            }
        }

        // Zoom level for current scale
        public int ZoomLevel
        {
            get
            {
                return zoomLevel;
            }
            set
            {
                if (zoomLevel != value)
                {
                    zoomLevel = value;
                    scale = 1 << zoomLevel;
                }
            }
        }

        // Fractional zoom
        public float Zoom
        {
            get 
            {
                return (float)(Math.Log(Scale) / Math.Log(2));
            }
            set 
            {
                Scale = (float)Math.Pow(2, value);
            }
        }

        // Scale relative to zoom level.
        public float ZoomScale
        {
            get
            {
                return Scale / (1 << ZoomLevel);
            }
        }

        public Camera()
        {
            Scale = 1;
            X = 0.5f;
            Y = 0.5f;
            ZoomLevel = 0;
            Bearing = 0;
            Tilt = 0;
            Roll = 0;
        }

        public Camera(float latitude, float longitude, float scale)
        {
            SetViewPosition(latitude, longitude);
            Scale = scale;
        }
                
        public void SetViewPosition(GeoPoint geoPoint)
        {
            SetViewPosition(geoPoint.Latitude, geoPoint.Longitude);
        }

        /// <summary>
        /// Set this view position to new coordinates
        /// </summary>
        /// <param name="latitude">Latitude of new view position</param>
        /// <param name="longitude">Longitude of new view position</param>
        public void SetViewPosition(float latitude, float longitude)
        {
            latitude = MercatorProjection.LimitLatitude(latitude);
            longitude = MercatorProjection.LimitLongitude(longitude);
            X = MercatorProjection.LongitudeToX(longitude);
            Y = MercatorProjection.LatitudeToY(latitude);
        }

        /// <summary>
        /// Copy an other view position to this one
        /// </summary>
        /// <param name="other">Other view position</param>
        public void Copy(Camera other)
        {
            X = other.X;
            Y = other.Y;

            Bearing = other.Bearing;
            Scale = other.Scale;
            ZoomLevel = other.ZoomLevel;
            Tilt = other.Tilt;
            Roll = other.Roll;
        }

        public void SetViewPosition(float x, float y, float scale, float bearing, float tilt)
        {
            X = x;
            Y = y;
            Scale = scale;

            Bearing = bearing.ClampToDegree();
            Tilt = tilt;
            ZoomLevel = MathExtensions.Log2((int)scale);
        }

        public void Set(float x, float y, float scale, float bearing, float tilt, float roll)
        {
            SetViewPosition(x, y, scale, bearing, tilt);
            this.roll = roll.ClampToDegree();
        }

        public GeoPoint getGeoPoint()
        {
            return new GeoPoint(MercatorProjection.ToLatitude(Y),
                    MercatorProjection.ToLongitude(X));
        }

        public float Latitude
        {
            get
            {
                return MercatorProjection.ToLatitude(Y);
            }
        }

        public float Longitude
        {
            get
            {
                return MercatorProjection.ToLongitude(X);
            }
        }

        public void SetByBoundingBox(GeoBox bbox, int viewWidth, int viewHeight)
        {
            float minx = MercatorProjection.LongitudeToX(bbox.MinLongitude);
            float miny = MercatorProjection.LatitudeToY(bbox.MaxLatitude);

            float dx = Math.Abs(MercatorProjection.LongitudeToX(bbox.MaxLongitude) - minx);
            float dy = Math.Abs(MercatorProjection.LatitudeToY(bbox.MinLatitude) - miny);
            float zx = viewWidth / (dx * TileExtension.TileSize);
            float zy = viewHeight / (dy * TileExtension.TileSize);

            scale = (float)Math.Min(zx, zy);
            zoomLevel = MathExtensions.Log2((int)scale);
            X = minx + dx / 2;
            Y = miny + dy / 2;
            Bearing = 0;
            Tilt = 0;
            Roll = 0;
        }

        public override string ToString()
        {
            return $"[X={X},Y={Y},Z={ZoomLevel},Lat={MercatorProjection.ToLatitude(Y)}/Lon={MercatorProjection.ToLongitude(X)}]";
        }
    }
}
