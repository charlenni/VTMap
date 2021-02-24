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
        // Timer for rendering loop
        Timer _timer;

        bool _drawing = false;
        object _syncObject = new object();

        
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
        }

        void Draw(SKCanvas canvas)
        {
            if (_drawing)
                return;

            lock (_syncObject)
            {
                _drawing = true;

                canvas.Clear(SKColors.LightGray);

                foreach (var renderer in Map.Layers.Renderers)
                    renderer.Draw(canvas, Viewport);

                NeedsRedraw = false;

                _drawing = false;
            }
        }

        void InternalSizeChanged(float width, float height)
        {
            Viewport.SizeChanged(width, height);
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
        }

        void OnDoubleTapped(object sender, TapEventArgs e)
        {
        }

        void OnLongPressing(object sender, TapEventArgs e)
        {
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
    }
}
