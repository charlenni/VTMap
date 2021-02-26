using Serilog;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VectorTilesView.Overlays.ScaleBar;
using VTMap.Core.Enums;
using VTMap.Core.Layers;

namespace VTMap.View.Overlays.ScaleBar
{
    /// <summary>
    /// A ScaleBarWidget displays the ratio of a distance on the map to the corresponding distance on the ground.
    /// It uses always the center of a given Viewport to calc this ratio.
    ///
    /// Usage
    /// To show a ScaleBarWidget, add a instance of the ScaleBarWidget to Map.Widgets by
    /// 
    ///   map.Widgets.Add(new ScaleBarWidget(map));
    ///   
    /// Customize
    /// ScaleBarMode: Determins, how much scalebars are shown. Could be Single or Both.
    /// SecondaryUnitConverter: First UnitConverter for upper scalebar. There are UnitConverters for metric, imperial and nautical units.
    /// SecondaryUnitConverter = NauticalUnitConverter.Instance });
    /// MaxWidth: Maximal width of the scalebar. Real width could be smaller.
    /// HorizontalAlignment: Where the ScaleBarWidget is shown. Could be Left, Right, Center or Position.
    /// VerticalAlignment: Where the ScaleBarWidget is shown. Could be Top, Bottom, Center or Position.
    /// PositionX: If HorizontalAlignment is Position, this value determins the distance to the left
    /// PositionY: If VerticalAlignment is Position, this value determins the distance to the top
    /// TextColor: Color for text and lines
    /// Halo: Color used around text and lines, so the scalebar is better visible
    /// TextAlignment: Alignment of scalebar text to the lines. Could be Left, Right or Center
    /// TextMargin: Space between text and lines of scalebar
    /// Font: Font which is used to draw text
    /// TickLength: Length of the ticks at scalebar
    /// </summary>
    public class ScaleBarOverlay : Overlay, INotifyPropertyChanged
    {
        /// <summary>
        /// Layer to be used for projection
        /// </summary>
        private readonly Layer _layer;

        ///
        /// Default position of the scale bar.
        ///
        private static readonly HorizontalAlignment DefaultScaleBarHorizontalAlignment = HorizontalAlignment.Left;
        private static readonly VerticalAlignment DefaultScaleBarVerticalAlignment = VerticalAlignment.Bottom;
        private static readonly Alignment DefaultScaleBarAlignment = Alignment.Left;
        private static readonly ScaleBarMode DefaultScaleBarMode = ScaleBarMode.Single;
        private static readonly SKFont DefaultFont = new SKFont(SKTypeface.Default, 10);

        public ScaleBarOverlay(Layer layer, float opacity = 1)
        {
            _layer = layer;

            _renderer = new ScaleBarOverlayRenderer(this, opacity);

            HorizontalAlignment = DefaultScaleBarHorizontalAlignment;
            VerticalAlignment = DefaultScaleBarVerticalAlignment;
            MarginX = 20;
            MarginY = 20;

            _maxWidth = 100;
            _height = 100;
            _textAlignment = DefaultScaleBarAlignment;
            _scaleBarMode = DefaultScaleBarMode;

            _unitConverter = MetricUnitConverter.Instance;
        }

        /// <summary>
        /// Layer to whom this scale bar belongs
        /// </summary>
        public Layer Layer { get => _layer; }

        float _maxWidth;

        /// <summary>
        /// Maximum usable width for scalebar. The real used width could be less, because we 
        /// want only integers as text.
        /// </summary>
        public float MaxWidth
        {
            get => _maxWidth;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_maxWidth == value)
                    return;

                _maxWidth = value;
                OnPropertyChanged();
            }
        }

        float _height;

        /// <summary>
        /// Real height of scalebar. Depends on number of unit converters and text size.
        /// Is calculated by renderer.
        /// </summary>
        public float Height
        {
            get => _height;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_height == value)
                    return;

                _height = value;
                OnPropertyChanged();
            }
        }

        SKColor _textColor = new SKColor(0, 0, 0);

        /// <summary>
        /// Foreground color of scalebar and text
        /// </summary>
        public SKColor TextColor
        {
            get => _textColor;
            set
            {
                if (_textColor == value)
                    return;
                _textColor = value;
                OnPropertyChanged();
            }
        }

        SKColor _haloColor = new SKColor(255, 255, 255);

        /// <summary>
        /// Halo color of scalebar and text, so that it is better visible
        /// </summary>
        public SKColor HaloColor
        {
            get => _haloColor;
            set
            {
                if (_haloColor == value)
                    return;
                _haloColor = value;
                OnPropertyChanged();
            }
        }

        public float Scale { get; } = 1;

        /// <summary>
        /// Stroke width for lines
        /// </summary>
        public float StrokeWidth { get; set; } = 2;

        /// <summary>
        /// Stroke width for halo of lines
        /// </summary>
        public float StrokeWidthHalo { get; set; } = 4;

        /// <summary>
        /// Length of the ticks
        /// </summary>
        public float TickLength { get; set; } = 3;

        Alignment _textAlignment;

        /// <summary>
        /// Alignment of text of scalebar
        /// </summary>
        public Alignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                if (_textAlignment == value)
                    return;

                _textAlignment = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Margin between end of tick and text
        /// </summary>
        public float TextMargin { get; } = 1;

        private SKFont _font = DefaultFont;

        /// <summary>
        /// Font to use for drawing text
        /// </summary>
        public SKFont Font
        {
            get => _font ?? DefaultFont;
            set
            {
                if (_font == value)
                    return;

                _font = value;
                OnPropertyChanged();
            }
        }

        private IUnitConverter _unitConverter;

        /// <summary>
        /// Normal unit converter for upper text. Default is MetricUnitConverter.
        /// </summary>
        public IUnitConverter UnitConverter
        {
            get => _unitConverter;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException($"{nameof(UnitConverter)} must not be null");
                }
                if (_unitConverter == value)
                {
                    return;
                }

                _unitConverter = value;
                OnPropertyChanged();
            }
        }

        private IUnitConverter _secondaryUnitConverter;
        
        /// <summary>
        /// Secondary unit converter for lower text if ScaleBarMode is Both. Default is ImperialUnitConverter.
        /// </summary>
        public IUnitConverter SecondaryUnitConverter
        {
            get => _secondaryUnitConverter;
            set
            {
                if (_secondaryUnitConverter == value)
                {
                    return;
                }

                _secondaryUnitConverter = value;
                OnPropertyChanged();
            }
        }

        private ScaleBarMode _scaleBarMode;

        /// <summary>
        /// ScaleBarMode of scalebar. Could be Single to show only one or Both for showing two units.
        /// </summary>
        public ScaleBarMode ScaleBarMode
        {
            get => _scaleBarMode;
            set
            {
                if (_scaleBarMode == value)
                {
                    return;
                }

                _scaleBarMode = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Draw a rectangle around the scale bar for testing
        /// </summary>
        public bool ShowBoundingBox { get; set; }

        protected override void OnPropertyChanged([CallerMemberName] string name = null)
        {
            if (name == nameof(TextColor) || name == nameof(HaloColor)
                || name == nameof(StrokeWidth) || name == nameof(StrokeWidthHalo)
                || name == nameof(Font))
                ((ScaleBarOverlayRenderer)_renderer).UpdatePaints();
            base.OnPropertyChanged(name);
        }

        public bool CanTransform()
        {
            if (_layer?.Projection == null) 
            {
                Log.Warning($"ScaleBarOverlay can't be drawn because of missing projection");
                return false;
            }
            return true;
        }
    }
}