using BruTile;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using VTMap.Core;
using VTMap.Core.Extensions;
using VTMap.Core.Projections;

namespace VTMap.View
{
    public class Viewport : INotifyPropertyChanged
    {
        public const int TileSize = 256;

        const int _maxZoomLevel = 24;
        const int _minZoomLevel = 0;
        const float VIEW_DISTANCE = 1.0f;
        const float VIEW_NEAR = 1;
        const float VIEW_FAR = 8;
        // Scale map plane at VIEW_DISTANCE to near plane
        const float VIEW_SCALE = VIEW_NEAR / VIEW_DISTANCE * 0.5f;
        const float MIN_TILT = 0;
        // Limited by possible number of tiles and cutting map on near and far plane
        const float MAX_TILT = 65;


        const float _minX = -1.0f;
        const float _maxX = 1.0f;
        const float _minY = -1.0f;
        const float _maxY = 1.0f;
        const float _minRotation = -180.0f;
        const float _maxRotation = 180.0f;
        const float _minRoll = -180.0f;
        const float _maxRoll = 180.0f;
        const float _minTilt = MIN_TILT;
        const float _maxTilt = MAX_TILT;
        const float _minScale = 1 << _minZoomLevel;
        const float _maxScale = 1 << _maxZoomLevel;

        double MathLog2 = Math.Log(2);

        float _pixelDensity;
        Point _center;
        Point _mapViewCenter;
        int _zoomLevel;
        float _rotation;
        float _roll;
        float _tilt;
        float _scale;
        float screenCenterX;
        float screenCenterY;

        List<TileIndex> _tiles;

        double[,] _matrix = new double[3, 3];

        Matrix4x4 _projMatrix = Matrix4x4.Identity;
        Matrix4x4 _projMatrixUnscaled = Matrix4x4.Identity;
        Matrix4x4 _projMatrixInverse = Matrix4x4.Identity;
        Matrix4x4 _viewProjMatrix = Matrix4x4.Identity;
        Matrix4x4 _unprojMatrix = Matrix4x4.Identity;
        Matrix4x4 _tempMatrix = Matrix4x4.Identity;

        SKMatrix _viewToScreenMatrix = SKMatrix.Identity;
        SKMatrix _screenToViewMatrix = SKMatrix.Identity;

        public event PropertyChangedEventHandler PropertyChanged;

        public Viewport()
        {
            _pixelDensity = 0;
            _scale = _minScale;
            _center = new Point(0.0f, 0.0f);
            _mapViewCenter = new Point(0.0f, 0.0f);
            _zoomLevel = MathExtensions.Log2((int)_scale);
            _rotation = 0f;
            _roll = 0;
            _tilt = 0;
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
                UpdateMatrices();
            }
        }

        // Width of viewport
        public float Width { get; private set; }

        // Height of viewport
        public float Height { get; private set; }

        // Center of viewport, projected position with X/Y in range -1.0..+1.0
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
                    _center.X = value.X.ClampToMinMax(_minX, _maxX);
                    _center.Y = value.Y.ClampToMinMax(_minY, _maxY);
                    OnPropertyChanged();
                }
            }
        }

        public Point MapViewCenter
        {
            get
            {
                return _mapViewCenter.Clone();
            }
            internal set
            {
                if (_mapViewCenter != value)
                {
                    _mapViewCenter.X = value.X.ClampToMinMax(-1f, +1f);
                    _mapViewCenter.Y = value.Y.ClampToMinMax(-1f, +1f);
                    OnPropertyChanged();
                }
            }
        }

        public float TileScaleFactor { get => _scale * TileSize / 2; }

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
            set
            {
                if (_rotation != value)
                {
                    _rotation = value.ClampToDegree();

                    OnPropertyChanged();
                }
            }
        }

        //Roll angle
        public float Roll
        {
            get
            {
                return _roll;
            }
            set
            {
                if (_roll != value)
                {
                    _roll = value.ClampToDegree();

                    OnPropertyChanged();
                }
            }
        }

        //Tilt angle
        public float Tilt
        {
            get
            {
                return _tilt;
            }
            set
            {
                if (_tilt != value)
                {
                    _tilt = value.ClampToMinMax(_minTilt, _maxTilt);

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

        // Get the minimal axis-aligned BoundingBox that encloses
        // the visible part of the map. Sets box to map coordinates:
        // xmin,ymin,xmax,ymax
        public Box BoundingBox(int expand)
        {
            var result = new Box();

            var array = MapExtents(expand);

            result.MinX = array[0].X;
            result.MaxX = array[0].X;
            result.MinY = array[0].Y;
            result.MaxY = array[0].Y;

            for (int i = 1; i < 4; i++)
            {
                result.MinX = Math.Min(result.MinX, array[i].X);
                result.MaxX = Math.Max(result.MaxX, array[i].X);
                result.MinY = Math.Min(result.MinY, array[i].Y);
                result.MaxY = Math.Max(result.MaxY, array[i].Y);
            }

            var cs = TileScaleFactor;
            var cx = _center.X * cs;
            var cy = _center.Y * cs;

            result.MinX = (cx + result.MinX) / cs;
            result.MaxX = (cx + result.MaxX) / cs;
            result.MinY = (cy + result.MinY) / cs;
            result.MaxY = (cy + result.MaxY) / cs;

            return result;
        }

        public List<TileIndex> Tiles
        {
            get
            {
                if (_tiles is null)
                    CreateTiles();
                return _tiles;
            }
        }

        public Point FromScreenToView(Point screen)
        {
            (var x, var y) = UnprojectScreen(screen.X, screen.Y);

            double cs = TileScaleFactor;
            double cx = _center.X * cs;
            double cy = _center.Y * cs;

            double dx = cx + x;
            double dy = cy + y;

            dx /= cs;
            dy /= cs;

            while (dx > 1)
                dx -= 1;
            while (dx < 0)
                dx += 1;

            if (dy > 1)
                dy = 1;
            else if (dy < 0)
                dy = 0;

            return new Point(dx, dy);
        }

        public Point FromViewToScreen(Point view, bool relativeToCenter = false)
        {
            double cs = TileScaleFactor;
            double cx = _center.X * cs;
            double cy = _center.Y * cs;

            var vec = new Vector4((float)(view.X * cs - cx), (float)(view.Y * cs - cy), 0, 1);

            vec = new Vector4(
                        _viewProjMatrix.M11 * vec.X + _viewProjMatrix.M12 * vec.Y + _viewProjMatrix.M13 * vec.Z + _viewProjMatrix.M14 * vec.W,
                        _viewProjMatrix.M21 * vec.X + _viewProjMatrix.M22 * vec.Y + _viewProjMatrix.M23 * vec.Z + _viewProjMatrix.M24 * vec.W,
                        _viewProjMatrix.M31 * vec.X + _viewProjMatrix.M32 * vec.Y + _viewProjMatrix.M33 * vec.Z + _viewProjMatrix.M34 * vec.W,
                        _viewProjMatrix.M41 * vec.X + _viewProjMatrix.M42 * vec.Y + _viewProjMatrix.M43 * vec.Z + _viewProjMatrix.M44 * vec.W
                        );

            if (relativeToCenter)
                return new Point(vec.X * screenCenterX, -(vec.Y * screenCenterY));

            return new Point(vec.X * screenCenterX + screenCenterX, -(vec.Y * screenCenterY) + screenCenterY);
        }
       
        public override string ToString()
        {
            return $"[Center={Center.X}/{Center.Y},Z={ZoomLevel},Lat={MercatorProjection.ToLatitude(Center.Y)}/Lon={MercatorProjection.ToLongitude(Center.X)}]";
        }

        internal void SizeChanged(double width, double height)
        {
            Width = (float)width;
            Height = (float)height;

            screenCenterX = Width / 2.0f;
            screenCenterY = Height / 2.0f;

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
            _roll = 0;
            _tilt = 0;

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
            _roll = other.Roll;
            _tilt = other.Tilt;
            _scale = other.Scale;
            _zoomLevel = other.ZoomLevel;

            OnPropertyChanged();
        }

        void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyName == "Size")
                UpdateProjectionMatrices();

            UpdateMatrices();
            CreateTiles();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        void CreateTiles()
        {
            _tiles = new List<TileIndex>();

            var box = BoundingBox(0);
            var numberOfTiles = (int)Math.Pow(2, _zoomLevel);
            var minY = Math.Max(Math.Min(box.MinY, _maxY), _minY);
            var maxY = Math.Min(Math.Max(box.MaxY, _minY), _maxY);

            if (minY > 1.0 || maxY < -1.0)
                return;

            var minTileX = (int)((float)Math.Floor(numberOfTiles * (1.0 + box.MinX) / 2)).ClampToMinMax(0, numberOfTiles);
            var maxTileX = (int)((float)Math.Ceiling(numberOfTiles * (1.0 + box.MaxX) / 2)).ClampToMinMax(0, numberOfTiles);
            var minTileY = (int)((float)Math.Floor(numberOfTiles * (1.0 + box.MinY) / 2)).ClampToMinMax(0, numberOfTiles);
            var maxTileY = (int)((float)Math.Ceiling(numberOfTiles * (1.0 + box.MaxY) / 2)).ClampToMinMax(0, numberOfTiles);

            for (var y = minTileY; y < maxTileY; y++)
                for (var x = minTileX; x < maxTileX; x++)
                    _tiles.Add(new TileIndex(x, y, _zoomLevel));
        }

        public GeoPoint CenterAsGeoPoint()
        {
            return new GeoPoint(MercatorProjection.ToLatitude(_center.Y),
                    MercatorProjection.ToLongitude(_center.X));
        }

        void UpdateProjectionMatrices()
        {
            // Setup projection matrix:
            // 0. Scale to window coordinates
            // 1. Translate to VIEW_DISTANCE
            // 2. Apply projection
            // Setup inverse projection:
            // 0. Invert projection
            // 1. Invert translate to VIEW_DISTANCE

            float ratio = (Height / Width) * VIEW_SCALE;

            _projMatrix = Frustum(-VIEW_SCALE, VIEW_SCALE, ratio, -ratio, VIEW_NEAR, VIEW_FAR);

            _tempMatrix = Matrix4x4.CreateTranslation(0, 0, -VIEW_DISTANCE);
            
            _projMatrix = Matrix4x4.Multiply(_projMatrix, _tempMatrix);

            // Set inverse projection matrix (without scaling)
            if (!Matrix4x4.Invert(_projMatrix, out _projMatrixInverse))
                throw new ArithmeticException("Inverse of projection matrix invalide");

            _projMatrixUnscaled = _projMatrix;

            // Scale to window coordinates
            _tempMatrix = Matrix4x4.CreateScale(1 / Width, 1 / Width, 1 / Width);
            _projMatrix = Matrix4x4.Multiply(_projMatrix, _tempMatrix);

            UpdateMatrices();
        }

        void UpdateMatrices()
        {
            // View matrix:
            // 0. Apply yaw
            // 1. Apply roll
            // 2. Apply pitch

            var rotationMatrix = Matrix4x4.CreateRotationZ(_rotation.ToRadians());
            
            _tempMatrix = Matrix4x4.CreateRotationY(_roll.ToRadians());
            rotationMatrix = Matrix4x4.Multiply(_tempMatrix, rotationMatrix);

            _tempMatrix = Matrix4x4.CreateRotationX(_tilt.ToRadians());
            rotationMatrix = Matrix4x4.Multiply(_tempMatrix, rotationMatrix);

            var viewMatrix = rotationMatrix; 

            _tempMatrix = Matrix4x4.CreateTranslation(_mapViewCenter.X * Width, _mapViewCenter.Y * Height, 0);
            viewMatrix = Matrix4x4.Multiply(_tempMatrix, viewMatrix);

            _viewProjMatrix = Matrix4x4.Multiply(_projMatrix, viewMatrix);

            if (!Matrix4x4.Invert(_viewProjMatrix, out _unprojMatrix))
                throw new ArithmeticException("Inverse of unprojection matrix invalide");
        }

        (float, float) Unproject(float x, float y)
        {
            // Get point for near / opposite plane
            Vector4 vec = new Vector4(x, y, -1, 1);

            vec = new Vector4(
                        _unprojMatrix.M11 * vec.X + _unprojMatrix.M12 * vec.Y + _unprojMatrix.M13 * vec.Z + _unprojMatrix.M14 * vec.W,
                        _unprojMatrix.M21 * vec.X + _unprojMatrix.M22 * vec.Y + _unprojMatrix.M23 * vec.Z + _unprojMatrix.M24 * vec.W,
                        _unprojMatrix.M31 * vec.X + _unprojMatrix.M32 * vec.Y + _unprojMatrix.M33 * vec.Z + _unprojMatrix.M34 * vec.W,
                        _unprojMatrix.M41 * vec.X + _unprojMatrix.M42 * vec.Y + _unprojMatrix.M43 * vec.Z + _unprojMatrix.M44 * vec.W
                        );

            double nx = vec.X;
            double ny = vec.Y;
            double nz = vec.Z;

            // Get point for far plane
            vec.X = x;
            vec.Y = y;
            vec.Z = 1;
            vec.W = 1;

            vec = new Vector4(
                        _unprojMatrix.M11 * vec.X + _unprojMatrix.M12 * vec.Y + _unprojMatrix.M13 * vec.Z + _unprojMatrix.M14 * vec.W,
                        _unprojMatrix.M21 * vec.X + _unprojMatrix.M22 * vec.Y + _unprojMatrix.M23 * vec.Z + _unprojMatrix.M24 * vec.W,
                        _unprojMatrix.M31 * vec.X + _unprojMatrix.M32 * vec.Y + _unprojMatrix.M33 * vec.Z + _unprojMatrix.M34 * vec.W,
                        _unprojMatrix.M41 * vec.X + _unprojMatrix.M42 * vec.Y + _unprojMatrix.M43 * vec.Z + _unprojMatrix.M44 * vec.W
                        );

            double fx = vec.X;
            double fy = vec.Y;
            double fz = vec.Z;

            // Calc differences
            double dx = fx - nx;
            double dy = fy - ny;
            double dz = fz - nz;

            double dist;

            if (y > 0 && nz < dz && fz > dz)
            {
                // Keep far distance (y > 0), while world flips between the screen coordinates.
                // Screen coordinates can't be correctly converted to map coordinates
                // as map plane doesn't intersect with top screen corners.
                dist = 1; // Far plane
            }
            else if (y < 0 && fz < dz && nz > dz)
            {
                // Keep near distance (y < 0), while world flips between the screen coordinates.
                // Screen coordinates can't be correctly converted to map coordinates
                // as map plane doesn't intersect with bottom screen corners.
                dist = 0; // Near plane
            }
            else
            {
                // Calc factor to get map coordinates on current distance
                dist = Math.Abs(-nz / dz);
                if (double.IsNaN(dist) || dist > 1)
                {
                    // Limit distance as it may exceeds to infinity
                    dist = 1; // Far plane
                }
            }

            // near + dist * (far - near)
            return ((float)(nx + dist * dx), (float)(ny + dist * dy));
        }

        (float, float) UnprojectScreen(double x, double y)
        {
            // Scale to -1..1
            float scaledX = (float)(1 - (x / Width * 2));
            float scaledY = (float)(1 - (y / Height * 2));

            return Unproject(-scaledX, scaledY);
        }

        // Get the inverse projection of the viewport, i.e. the
        // coordinates with z==0 that will be projected exactly
        // to screen corners by current view-projection-matrix.
        // Except when screen corners don't hit the map (e.g. on large tilt),
        // then it will return the intersection with near and far plane.
        // 0 -> x,y bottom-right,
        // 1 -> x,y bottom-left,
        // 2 -> x,y top-left,
        // 3 -> x,y top-right.
        // Add: Increase extents of box
        public Point[] MapExtents(float add)
        {
            Point[] result = new Point[4];
            float x, y;

            // bottom-right
            (x, y) = Unproject(1, -1);
            result[0] = new Point(x, y);
            // bottom-left 
            (x, y) = Unproject(-1, -1);
            result[1] = new Point(x, y);
            // top-left
            (x, y) = Unproject(-1, 1);
            result[2] = new Point(x, y);
            // top-right
            (x, y) = Unproject(1, 1);
            result[3] = new Point(x, y);

            if (add == 0)
                return result;

            for (int i = 0; i < 4; i++)
            {
                x = result[i].X;
                y = result[i].Y;
                float len = (float)MathExtensions.Hypot(x, y);
                result[i].X += x / len * add;
                result[i].Y += y / len * add;
            }

            return result;
        }

        // See https://docs.microsoft.com/de-de/windows/win32/opengl/glfrustum?redirectedfrom=MSDN
        Matrix4x4 Frustum(float left, float right, float bottom, float top, float near, float far)
        {
            if (left == right)
                throw new ArgumentException("left == right");
            if (top == bottom)
                throw new ArgumentException("top == bottom");
            if (near == far)
                throw new ArgumentException("near == far");
            if (near <= 0.0f)
                throw new ArgumentException("near <= 0.0f");
            if (far <= 0.0f)
                throw new ArgumentException("far <= 0.0f");

            float tempWidth = 1.0f / (right - left);
            float tempHeight = 1.0f / (top - bottom);
            float tempDepth = 1.0f / (near - far);
            float x = 2.0f * (near * tempWidth);
            float y = 2.0f * (near * tempHeight);
            float A = (right + left) * tempWidth;
            float B = (top + bottom) * tempHeight;
            float C = (far + near) * tempDepth;
            float D = 2.0f * (far * near * tempDepth);

            return new Matrix4x4( x, 0f,   A, 0f,
                                 0f,  y,   B, 0f,
                                 0f, 0f,   C,  D,
                                 0f, 0f, -1f, 0f);
        }
    }
}
