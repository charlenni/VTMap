using BruTile;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VTMap.Core;
using VTMap.Core.Extensions;
using VTMap.Core.Primitives;

namespace VTMap.View
{
    public class Viewport : INotifyPropertyChanged
    {
        public const int TileSize = 512;

        const int _maxZoomLevel = 24;
        const int _minZoomLevel = 0;

        const float _minX = -1.0f;
        const float _maxX = 1.0f;
        const float _minY = -1.0f;
        const float _maxY = 1.0f;
        const float _minRotation = -180.0f;
        const float _maxRotation = 180.0f;
        const float _minScale = 1 << _minZoomLevel;
        const float _maxScale = 1 << _maxZoomLevel;

        double MathLog2 = Math.Log(2);

        float _pixelDensity;
        Point _center;
        Point _mapViewCenter;
        int _zoomLevel;
        float _rotation;
        float _scale;
        float screenCenterX;
        float screenCenterY;

        List<TileIndex> _tiles;

        SKMatrix _screenToViewMatrix = SKMatrix.Identity;
        SKMatrix _viewToScreenMatrix = SKMatrix.Identity;

        public event PropertyChangedEventHandler PropertyChanged;

        public Viewport()
        {
            _pixelDensity = 0;
            _scale = _minScale;
            _center = new Point(0.0f, 0.0f);
            _mapViewCenter = new Point(0.0f, 0.0f);
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

        public float TileScaleFactor { get => _scale * TileSize; }

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

        // Get the minimal axis-aligned BoundingBox that encloses
        // the visible part of the map. Sets box to map coordinates:
        // xmin,ymin,xmax,ymax
        public Box BoundingBox(int expand = 0)
        {
            var result = new Box();

            var viewNW = _screenToViewMatrix.MapPoint(new SKPoint(0, 0));
            var viewNE = _screenToViewMatrix.MapPoint(new SKPoint(Width, 0));
            var viewSE = _screenToViewMatrix.MapPoint(new SKPoint(Width, Height));
            var viewSW = _screenToViewMatrix.MapPoint(new SKPoint(0, Height));

            result.MinX = MathExtensions.Min(viewNW.X, viewNE.X, viewSE.X, viewSW.X);
            result.MaxX = MathExtensions.Max(viewNW.X, viewNE.X, viewSE.X, viewSW.X);
            result.MinY = MathExtensions.Min(-viewNW.Y, -viewNE.Y, -viewSE.Y, -viewSW.Y);
            result.MaxY = MathExtensions.Max(-viewNW.Y, -viewNE.Y, -viewSE.Y, -viewSW.Y);

            return result;
        }

        public List<TileIndex> Tiles
        {
            get
            {
                if (_tiles is null)
                    UpdateTileList();
                return _tiles;
            }
        }

        /// <summary>
        /// Convert a given screen position to a view position in range of [-1..+1]
        /// </summary>
        /// <param name="view">Point in screen coordinates</param>
        /// <param name="relativeToCenter">If true, then function returns the distance between center of map and given point</param>
        /// <returns></returns>
        public Point FromScreenToView(Point screen, bool relativeToCenter = false)
        {
            var vec = _screenToViewMatrix.MapPoint(new SKPoint(screen.X, screen.Y));

            if (relativeToCenter)
                return new Point(_center.X - vec.X, -vec.Y + _center.Y);

            return new Point(vec.X, -vec.Y);
        }

        /// <summary>
        /// Convert a given view position in range [-1..+1] to a screen position
        /// </summary>
        /// <param name="view">Point in view coordinates</param>
        /// <param name="relativeToCenter">If true, then function returns the distance between center of screen and given point</param>
        /// <returns></returns>
        public Point FromViewToScreen(Point view, bool relativeToCenter = false)
        {
            var vec = _viewToScreenMatrix.MapPoint(new SKPoint(view.X, view.Y));

            if (relativeToCenter)
                return new Point(vec.X - screenCenterX, vec.Y - screenCenterY);

            return new Point(vec.X, vec.Y);
        }

        /// <summary>
        /// Create a SKMatrix for the given tile index
        /// </summary>
        /// <param name="tile">Tile index for which to create the matrix</param>
        /// <returns></returns>
        public SKMatrix MatrixForTile(TileIndex tile)
        {
            var numberOfTiles = (float)Math.Pow(2, ZoomLevel);
            var scaleFactor = 2 * ZoomScale / TileScaleFactor;
            var viewNW = new Point(-1.0 + 2.0 * (tile.Col / numberOfTiles), -1.0 + 2.0 * (tile.Row / numberOfTiles));
            var screenNW = FromViewToScreen(viewNW);

            var matrix = SKMatrix.CreateScale(scaleFactor, scaleFactor);
            matrix = matrix.PostConcat(SKMatrix.CreateTranslation(viewNW.X, viewNW.Y));
            matrix = matrix.PostConcat(_viewToScreenMatrix);

            return matrix;
        }
       
        public override string ToString()
        {
            return $"[Center={Center.X}/{Center.Y},Z={ZoomLevel}]";
        }

        public void SizeChanged(double width, double height)
        {
            Width = (float)width;
            Height = (float)height;

            screenCenterX = Width / 2.0f;
            screenCenterY = Height / 2.0f;

            OnPropertyChanged("Size");
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
            UpdateMatrices();
            UpdateTileList();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        void UpdateTileList(float yAxis = -1)
        {
            _tiles = new List<TileIndex>();

            var box = BoundingBox();
            var numberOfTiles = (int)Math.Pow(2, _zoomLevel);
            // Change axes
            var minY = yAxis * box.MinY.ClampToMinMax(_minY, _maxY);
            var maxY = yAxis * box.MaxY.ClampToMinMax(_minY, _maxY);

            if (minY > maxY)
                (minY, maxY) = (maxY, minY);
                
            if (minY > 1.0 || maxY < -1.0)
                return;

            var minTileX = (int)((float)Math.Floor(numberOfTiles * (1.0 + box.MinX) / 2)).ClampToMinMax(0, numberOfTiles);
            var maxTileX = (int)((float)Math.Ceiling(numberOfTiles * (1.0 + box.MaxX) / 2)).ClampToMinMax(0, numberOfTiles);
            var minTileY = (int)((float)Math.Floor(numberOfTiles * (1.0 + minY) / 2)).ClampToMinMax(0, numberOfTiles);
            var maxTileY = (int)((float)Math.Ceiling(numberOfTiles * (1.0 + maxY) / 2)).ClampToMinMax(0, numberOfTiles);

            for (var y = minTileY; y < maxTileY; y++)
                for (var x = minTileX; x < maxTileX; x++)
                    _tiles.Add(new TileIndex(x, y, _zoomLevel));
        }

        void UpdateMatrices()
        {
            var viewScale = 2 / TileScaleFactor;

            _screenToViewMatrix = SKMatrix.CreateTranslation(-screenCenterX, -screenCenterY);
            _screenToViewMatrix = _screenToViewMatrix.PostConcat(SKMatrix.CreateRotationDegrees(_rotation));
            _screenToViewMatrix = _screenToViewMatrix.PostConcat(SKMatrix.CreateScale(viewScale, viewScale));
            _screenToViewMatrix = _screenToViewMatrix.PostConcat(SKMatrix.CreateTranslation(_center.X, -_center.Y));
            _viewToScreenMatrix = _screenToViewMatrix.Invert();
        }
    }
}
