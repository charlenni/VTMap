using SkiaSharp;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VTMap.Core;
using VTMap.Core.Extensions;
using VTMap.Core.Projections;

namespace VTMap.View
{
    public class Viewport : INotifyPropertyChanged
    {
        public const int TileSize = 512;

        const int _maxZoomLevel = 24;
        const int _minZoomLevel = 0;

        const float _minX = -0.5f;
        const float _maxX = 0.5f;
        const float _minY = -0.5f;
        const float _maxY = 0.5f;
        const float _minRotation = -180.0f;
        const float _maxRotation = 180.0f;
        const float _minScale = 1 << _minZoomLevel;
        const float _maxScale = 1 << _maxZoomLevel;

        double MathLog2 = Math.Log(2);

        float _pixelDensity;
        Point _center;
        int _zoomLevel;
        float _rotation;
        float _scale;

        SKMatrix _viewToScreenMatrix = SKMatrix.Identity;
        SKMatrix _screenToViewMatrix = SKMatrix.Identity;

        public event PropertyChangedEventHandler PropertyChanged;

        public Viewport()
        {
            _pixelDensity = 0;
            _scale = _minScale;
            _center = new Point(0.0f, 0.0f);
            _zoomLevel = MathExtensions.Log2((int)_scale);
            _rotation = 0f;
        }

        public float MinRotation { get => _minRotation; }
        public float MaxRotation { get => _maxRotation; }

        public bool Updating { get; internal set; } = false;

        /// <summary>
        /// PixelDensity of the canvas with pixels per device independet units
        /// </summary>
        public float PixelDensity 
        {
            get
            {
                return _pixelDensity;
            }
            internal set 
            {
                _pixelDensity = value;
                RecalcMatrices();
            }
        }

        // Width of viewport
        public float Width { get; private set; }

        // Height of viewport
        public float Height { get; private set; }

        // Center of viewport, projected position with X/Y in range -0.5..0.5
        public Point Center
        {
            get
            {
                return _center.Clone();
            }
            internal set
            {
                if (_center != value)
                {
                    _center.X = value.X;
                    _center.Y = value.Y;
                    OnPropertyChanged();
                }
            }
        }

        // Absolute scale
        public float Scale
        {
            get
            {
                return _scale;
            }
            internal set
            {
                if (_scale != value)
                {
                    _scale = value.ClampToMinMax(_minScale, _maxScale);
                    _zoomLevel = MathExtensions.Log2((int)_scale);
                    OnPropertyChanged();
                }
            }
        }

        //Rotation angle
        public float Rotation
        {
            get
            {
                return _rotation;
            }
            internal set
            {
                if (_rotation != value)
                {
                    _rotation = value.ClampToDegree();

                    OnPropertyChanged();
                }
            }
        }


        // Zoom level for current scale
        public int ZoomLevel
        {
            get
            {
                return _zoomLevel;
            }
            internal set
            {
                if (_zoomLevel != value)
                {
                    _zoomLevel = value;
                    _scale = 1 << _zoomLevel;

                    OnPropertyChanged();
                }
            }
        }

        // Fractional zoom
        public float Zoom
        {
            get
            {
                return (float)(Math.Log(_scale) / MathLog2);
            }
            internal set
            {
                _scale = (float)Math.Pow(2, value);

                OnPropertyChanged();
            }
        }

        // Scale relative to zoom level.
        public float ZoomScale
        {
            get
            {
                return _scale / (1 << _zoomLevel);
            }
        }

        public float Latitude
        {
            get
            {
                return MercatorProjection.ToLatitude(_center.Y);
            }
        }

        public float Longitude
        {
            get
            {
                return MercatorProjection.ToLongitude(_center.X);
            }
        }

        public SKMatrix ViewToScreenMatrix
        {
            get
            {
                return _viewToScreenMatrix;
            }
            internal set
            {
                if (_viewToScreenMatrix != value)
                {
                    _viewToScreenMatrix = value;
                    _screenToViewMatrix = _screenToViewMatrix.Invert();
                    OnPropertyChanged();
                }
            }
        }

        public SKMatrix ScreenToViewMatrix
        {
            get
            {
                return _screenToViewMatrix;
            }
            internal set 
            { 
                if (_screenToViewMatrix != value)
                {
                    _screenToViewMatrix = value;
                    _viewToScreenMatrix = _screenToViewMatrix.Invert();
                    OnPropertyChanged();
                }
            }
        }

        public Point FromScreenToView(Point screen)
        {
            var result = _screenToViewMatrix.MapPoint(new SKPoint(screen.X, screen.Y));
            return new Point(result.X, result.Y);
        }

        public Point FromViewToScreen(Point view)
        {
            var result = _viewToScreenMatrix.MapPoint(new SKPoint(view.X, view.Y));
            return new Point(result.X, result.Y);
        }

        public GeoPoint CenterAsGeoPoint()
        {
            return new GeoPoint(MercatorProjection.ToLatitude(_center.Y),
                    MercatorProjection.ToLongitude(_center.X));
        }

        public override string ToString()
        {
            return $"[Center={Center.X}/{Center.Y},Z={ZoomLevel},Lat={MercatorProjection.ToLatitude(Center.Y)}/Lon={MercatorProjection.ToLongitude(Center.X)}]";
        }

        internal void SizeChanged(double width, double height)
        {
            Width = (float)width;
            Height = (float)height;

            OnPropertyChanged("Size");
        }

        internal void SetCenter(GeoPoint geoPoint)
        {
            SetCenter(geoPoint.Latitude, geoPoint.Longitude);
        }

        /// <summary>
        /// Set this view position to new coordinates
        /// </summary>
        /// <param name="latitude">Latitude of new view position</param>
        /// <param name="longitude">Longitude of new view position</param>
        internal void SetCenter(float latitude, float longitude)
        {
            latitude = MercatorProjection.LimitLatitude(latitude);
            longitude = MercatorProjection.LimitLongitude(longitude);
            _center.X = MercatorProjection.LongitudeToX(longitude);
            _center.Y = MercatorProjection.LatitudeToY(latitude);

            OnPropertyChanged("Center");
        }

        internal void SetCenter(float x, float y, float scale, float rotation)
        {
            _center.X = x;
            _center.Y = y;
            _scale = scale;
            _rotation = rotation.ClampToDegree();
            _zoomLevel = MathExtensions.Log2((int)scale);

            OnPropertyChanged("Center");
        }

        internal void SetByBoundingBox(GeoBox bbox, int viewWidth, int viewHeight)
        {
            float minx = MercatorProjection.LongitudeToX(bbox.MinLongitude);
            float miny = MercatorProjection.LatitudeToY(bbox.MaxLatitude);

            float dx = Math.Abs(MercatorProjection.LongitudeToX(bbox.MaxLongitude) - minx);
            float dy = Math.Abs(MercatorProjection.LatitudeToY(bbox.MinLatitude) - miny);
            float zx = viewWidth / (dx * TileExtension.TileSize);
            float zy = viewHeight / (dy * TileExtension.TileSize);

            _scale = (float)Math.Min(zx, zy);
            _zoomLevel = MathExtensions.Log2((int)_scale);
            _center.X = minx + dx / 2;
            _center.Y = miny + dy / 2;
            _rotation = 0;

            OnPropertyChanged();
        }

        /// <summary>
        /// Copy an other view position to this one
        /// </summary>
        /// <param name="other">Other view position</param>
        public void Copy(Viewport other)
        {
            _center.X = other.Center.X;
            _center.Y = other.Center.Y;
            _rotation = other.Rotation;
            _scale = other.Scale;
            _zoomLevel = other.ZoomLevel;

            OnPropertyChanged();
        }

        void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            RecalcMatrices();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        void RecalcMatrices()
        {
            //var viewSize = _scale * TileSize;
            var viewScale = 1 / _scale / Width;

            //var viewCenterX = _center.X * viewSize;
            //var viewCenterY = _center.Y * viewSize;

            var screenCenterX = Width / 2.0f;
            var screenCenterY = Height / 2.0f;

            _screenToViewMatrix = SKMatrix.CreateScale(1f / PixelDensity, 1f / PixelDensity);
            _screenToViewMatrix = _screenToViewMatrix.PostConcat(SKMatrix.CreateTranslation(-screenCenterX, -screenCenterY));
            _screenToViewMatrix = _screenToViewMatrix.PostConcat(SKMatrix.CreateRotationDegrees(_rotation));
            _screenToViewMatrix = _screenToViewMatrix.PostConcat(SKMatrix.CreateScale(viewScale, viewScale));
            _screenToViewMatrix = _screenToViewMatrix.PostConcat(SKMatrix.CreateTranslation(_center.X, _center.Y));

            _viewToScreenMatrix = _screenToViewMatrix.Invert();
        }
    }
}
