using SkiaSharp;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using VTMap.Core.Events;
using VTMap.Core.Map;
using VTMap.Core.Utilities;

namespace VTMap.View
{
    /// <summary>
    /// Common part for all platform dependent views
    /// </summary>
    public partial class VTMapView
    {
        // Values for drawing loop
        readonly Stopwatch _stopWatch = new Stopwatch();
        double _fpsAverage = 0.0;
        const double _fpsWanted = 60.0;
        int _fpsCount = 0;

        // Begin: All this is for the demos
        SKColor[,] colors = new SKColor[10, 10];
        int singleX = -1, singleY = -1;
        SKColor singleColor = SKColors.Blue;
        Animation singleAnimation;
        int doubleX = -1, doubleY = -1;
        int longX = -1, longY = -1;
        // End: All this is for the demos

        bool _drawing = false;
        object _syncObject = new Object();

        // Drawing matrix
        SKMatrix _drawMatrix = SKMatrix.Identity;
        // Timer for rendering loop
        Timer _timer;
        
        public MapData Map { get; set; }
        public Viewport Viewport { get; private set; }
        public Navigator Navigator { get; private set; }
        public ITouchEventHandler TouchEventHandler { get; private set; }

        /// <summary>
        /// Flag for redrawing view
        /// </summary>
        public bool NeedsRedraw { get; set; } = true;

        void InternalInit()
        {
            Map = new MapData();
            Viewport = new Viewport();
            Navigator = new Navigator(Viewport);

            // Register viewport events
            Viewport.PropertyChanged += OnViewportChanged;

            // Create timer for redrawing
            _timer = new Timer(OnTimerCallback, null, TimeSpan.FromMilliseconds(1000.0 / _fpsWanted), TimeSpan.FromMilliseconds(1000.0 / _fpsWanted));

            // Register all touch events
            TouchEventHandler.TouchDown += OnTouchDown;
            TouchEventHandler.SingleTapped += OnSingleTapped;
            TouchEventHandler.DoubleTapped += OnDoubleTapped;
            TouchEventHandler.LongPressing += OnLongPressing;
            TouchEventHandler.Pinching += OnPinching;
            TouchEventHandler.Pinched += OnPinched;
            TouchEventHandler.Panning += OnPanning;
            TouchEventHandler.Panned += OnPanned;
            TouchEventHandler.Swiped += OnSwiped;
            TouchEventHandler.WheelChanged += OnWheelChanged;

            // Begin: This is for demo only
            CreateColorsForDemo();
            singleAnimation = new Animation(1000);
            singleAnimation.Loop = true;
            singleAnimation.Entries.Add(new AnimationEntry(SKColors.Blue, SKColors.LightBlue,
                0, 1,
                Easing.SinInOut,
                (sender, value) => { 
                    singleColor = BlendColors((SKColor)sender.Start, (SKColor)sender.End, sender.Easing.Ease(value) * 2); 
                },
                (sender) => { 
                    singleColor = SKColors.Blue; 
                }));
            // End: This is for demo only
        }

        void Draw(SKCanvas canvas)
        {
            if (_drawing)
                return;

            lock (_syncObject)
                _drawing = true;

            // TODO: Find a better place to set this.
            // TODO: Isn't needed each time we draw
            if (Viewport.PixelDensity <= 0)
            {
                Viewport.SizeChanged(GetCanvasSize().Width, GetCanvasSize().Height);
                Viewport.PixelDensity = GetPixelDensity();
            }

            canvas.Clear(SKColors.LightGray);

            var paintDemo = new SKPaint { Color = SKColors.Blue, Style = SKPaintStyle.Stroke, StrokeWidth = 1f, };

            canvas.DrawLine(Viewport.Width / 2 - 50, Viewport.Height / 2, Viewport.Width / 2 + 50, Viewport.Height / 2, paintDemo);
            canvas.DrawLine(Viewport.Width / 2, Viewport.Height / 2 - 50, Viewport.Width / 2, Viewport.Height / 2 + 50, paintDemo);

            //DrawForDemo(canvas);

            var tiles = Viewport.Tiles.AsReadOnly();
            var tileScale = 1; // 4096 / Viewport.TileSize;

            var paint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Stroke, StrokeWidth = 1, };
            var paintText = new SKPaint { Color = SKColors.Green, IsStroke = false, TextSize = Viewport.TileSize / 16, TextAlign = SKTextAlign.Center, };

            foreach (var tile in tiles)
            {
                // Get matrix for tile
                var matrix = Viewport.MatrixForTile(tile);

                matrix = matrix.PreConcat(SKMatrix.CreateScale(1 / tileScale, 1 / tileScale));

                canvas.SetMatrix(matrix);

                var points = new SKPoint[4];

                points[0] = new SKPoint(0, 0);
                points[1] = new SKPoint(Viewport.TileSize, 0);
                points[2] = new SKPoint(Viewport.TileSize, Viewport.TileSize);
                points[3] = new SKPoint(0, Viewport.TileSize);

                var path = new SKPath();
                path.AddPoly(points, true);

                canvas.DrawPath(path, paint);

                canvas.DrawCircle(Viewport.TileSize / 8, Viewport.TileSize / 8, Viewport.TileSize / 16, paint);

                var text = $"Tile {tile.Col}/{tile.Row}/{tile.Level}";
                var width = paintText.MeasureText(text);

                canvas.DrawText(text, width / 2, Viewport.TileSize / 16, paintText); // 0.1 / Viewport.Scale), paintText);
                canvas.DrawText(text, width / 2, Viewport.TileSize - Viewport.TileSize / 64, paintText);
                canvas.DrawText(text, Viewport.TileSize - width / 2, Viewport.TileSize / 16, paintText);
                canvas.DrawText(text, Viewport.TileSize - width / 2, Viewport.TileSize - Viewport.TileSize / 64, paintText);

                // Draw text always horizontical
                canvas.SetMatrix(matrix.PreConcat(SKMatrix.CreateRotationDegrees(Viewport.Rotation, Viewport.TileSize / 2, Viewport.TileSize / 2)));
                canvas.DrawText(text, Viewport.TileSize / 2, Viewport.TileSize / 2, paintText);
            }

            NeedsRedraw = false;

            lock (_syncObject)
                _drawing = false;
        }

        void OnSizeChanged(object sender, EventArgs e)
        {
            Viewport.PixelDensity = GetPixelDensity();
            Viewport.SizeChanged(Width, Height);
        }

        private void OnViewportChanged(object sender, PropertyChangedEventArgs e)
        {
            NeedsRedraw = true;
        }

        void OnTouchDown(object sender, TouchEventArgs e)
        {
            Navigator.CancelAnimations();
        }

        void OnPinching(object sender, PinchEventArgs e)
        {
            Navigator.MoveBy(e.Translation.X, e.Translation.Y);
            Navigator.RotateBy(e.Rotation, e.MidPoint);
            Navigator.ScaleBy(e.Scale, e.MidPoint);
        }

        void OnPinched(object sender, PinchEventArgs e)
        {
        }

        void OnPanning(object sender, PanEventArgs e)
        {
            Navigator.MoveBy(e.NewPoint.X - e.PreviousPoint.X, e.NewPoint.Y - e.PreviousPoint.Y);
        }

        void OnPanned(object sender, PanEventArgs e)
        {
        }

        void OnSwiped(object sender, SwipeEventArgs e)
        {
            Navigator.SwipeWith(e.TranslationVelocity, 500);
        }

        void OnSingleTapped(object sender, TapEventArgs e)
        {
            CheckTap(e, ref singleX, ref singleY);

            if (singleX == -1 && singleY == -1)
                singleAnimation.Stop();
            else
                singleAnimation.Start();
        }

        void OnDoubleTapped(object sender, TapEventArgs e)
        {
            CheckTap(e, ref doubleX, ref doubleY);
        }

        void OnLongPressing(object sender, TapEventArgs e)
        {
            CheckTap(e, ref longX, ref longY);
        }

        void CheckTap(TapEventArgs e, ref int valueX, ref int valueY)
        { 
            var point = Viewport.FromScreenToView(e.Location);
            var x = (int)Math.Floor((point.X + 1.0) / 0.1);
            var y = (int)Math.Floor((point.Y + 1.0) / 0.1);

            if (x >= 0 && x < 10 && y >= 0 && y < 10)
                if (valueX == x && valueY == y)
                    valueX = valueY = -1;
                else
                {
                    valueX = x;
                    valueY = y;
                }

            NeedsRedraw = true;
        }

        void OnWheelChanged(object sender, WheelChangedEventArgs e)
        {
            var scale = (float)(e.Delta > 0 ? Math.Pow(1.1, e.Delta / 120) : Math.Pow(0.9, -e.Delta/120));
            Navigator.ScaleBy(scale, e.Location);
        }

        // See http://codetips.nl/skiagameloop.html
        void OnTimerCallback(object state)
        {
            // Get the elapsed time from the stopwatch because the 1/fps timer interval is not accurate and can be off by 2 ms
            var dt = _stopWatch.Elapsed.TotalSeconds;

            // Restart the time measurement for the next time this method is called
            _stopWatch.Restart();

            // Workload in background
            var redraw = NeedsRedraw || Animation.UpdateAnimations();

            // Calculate current fps
            var fps = dt > 0 ? 1.0 / dt : 0;

            // When the fps is to low, reduce the load by skipping the frame
            if (fps < _fpsWanted / 2)
                return;

            // Calculate an averaged fps
            _fpsAverage += fps;
            _fpsCount++;

            if (_fpsCount == 20)
            {
                fps = _fpsAverage / _fpsCount;
                Debug.WriteLine($"FPS {fps.ToString("N3", CultureInfo.InvariantCulture)}");

                _fpsCount = 0;
                _fpsAverage = 0.0;
            }

            // Called if needed
            if (redraw)
                Invalidate();
        }

        #region DEMO

        void CreateColorsForDemo()
        {
            Random rand = new Random();

            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 10; j++)
                    colors[i, j] = new SKColor((byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255));
        }

        void DrawForDemo(SKCanvas canvas)
        {
            var paint = new SKPaint();

            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 10; j++)
                {
                    paint.Color = SKColors.White;
                    if (singleX == i && singleY == j)
                        paint.Color = singleColor;
                    if (doubleX == i && doubleY == j)
                        paint.Color = SKColors.Green;
                    if (longX == i && longY == j)
                        paint.Color = SKColors.Red;

                    var min = Viewport.FromViewToScreen(new Core.Point(-1.0 + 0.2 * i, -1.0 + 0.2 * j));
                    var max = Viewport.FromViewToScreen(new Core.Point(-0.8 + 0.2 * i, -0.8 + 0.2 * j));
                    canvas.DrawRect(-1.0f + 0.2f * i, -1.0f + 0.2f * j, 0.2f, 0.2f, paint);

                    paint.Color = colors[i, j];

                    min = Viewport.FromViewToScreen(new Core.Point(-0.990 + 0.2 * i, -0.990 + 0.2 * j));
                    max = Viewport.FromViewToScreen(new Core.Point(-0.810 + 0.2 * i, -0.810 + 0.2 * j));
                    canvas.DrawRect(-0.990f + 0.2f * i, -0.990f + 0.2f * j, 0.18f, 0.18f, paint);
                }
        }

        SKColor BlendColors(SKColor a, SKColor b, double value)
        {
            byte red, green, blue = 0;
            double t = value > 1 ? 2 - value : value;
            red = (byte)Math.Sqrt((1 - t) * Math.Pow(a.Red, 2) + t * Math.Pow(b.Red, 2));
            green = (byte)Math.Sqrt((1 - t) * Math.Pow(a.Green, 2) + t * Math.Pow(b.Green, 2));
            blue = (byte)Math.Sqrt((1 - t) * Math.Pow(a.Blue, 2) + t * Math.Pow(b.Blue, 2));
            return new SKColor(red, green, blue);
        }

        #endregion
    }
}
