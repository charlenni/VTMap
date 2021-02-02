using SkiaSharp;
using System;
using System.Threading;
using VTMap.Core.Events;
using VTMap.Core.Extensions;
using VTMap.Core.Map;
using VTMap.Core.Utilities;

namespace VTMap.View
{
    /// <summary>
    /// Common part for all platform dependent views
    /// </summary>
    public partial class VTMapView
    {
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
        // Animation for swipe
        Animation _swipeAnimation;
        // Timer for rendering loop
        Timer _timer;
        
        public MapData Map { get; set; }
        public Camera Viewport { get; private set; }
        public ITouchEventHandler TouchEventHandler { get; private set; }

        /// <summary>
        /// Flag for redrawing view
        /// </summary>
        public bool NeedsRedraw { get; set; } = true;

        void InternalInit()
        {
            Map = new MapData();
            Viewport = new Camera();

            // Create timer for redrawing
            _timer = new Timer(OnTimerCallback, null, TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16));

            // Create animation for swipe gestures
            _swipeAnimation = new Animation(500);

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

            canvas.Clear();
            canvas.SetMatrix(_drawMatrix);

            var paint = new SKPaint();
            DrawForDemo(canvas, paint);

            NeedsRedraw = false;

            lock (_syncObject)
                _drawing = false;
        }

        void OnTouchDown(object sender, TouchEventArgs e)
        {
            if (_swipeAnimation.IsRunning)
            {
                _swipeAnimation.Stop();

                NeedsRedraw = true;
            }
        }

        void OnPinching(object sender, PinchEventArgs e)
        {
            SKMatrix touchMatrix = SKMatrix.CreateTranslation(e.Translation.X, e.Translation.Y);
            touchMatrix = touchMatrix.PostConcat(SKMatrix.CreateRotation(e.Rotation, (float)e.MidPoint.X, (float)e.MidPoint.Y));
            touchMatrix = touchMatrix.PostConcat(SKMatrix.CreateScale(e.Scale, e.Scale, (float)e.MidPoint.X, (float)e.MidPoint.Y));
            _drawMatrix = _drawMatrix.PostConcat(touchMatrix);

            NeedsRedraw = true;
        }

        void OnPinched(object sender, PinchEventArgs e)
        {
        }

        void OnPanning(object sender, PanEventArgs e)
        {
            _drawMatrix = _drawMatrix.PostConcat(SKMatrix.CreateTranslation((float)(e.NewPoint.X - e.PreviousPoint.X), (float)(e.NewPoint.Y - e.PreviousPoint.Y)));

            NeedsRedraw = true;
        }

        void OnPanned(object sender, PanEventArgs e)
        {
        }

        void OnSwiped(object sender, SwipeEventArgs e)
        {
            _swipeAnimation.Entries.Clear();
            if (e.TranslationVelocity.X != 0 || e.TranslationVelocity.Y != 0)
            {
                _swipeAnimation.Entries.Add(CreateSwipeTranslationAnimationEntry(e.TranslationVelocity.X, e.TranslationVelocity.Y, _swipeAnimation.Duration));
            }
            _swipeAnimation.Start();
        }

        void OnSingleTapped(object sender, TapEventArgs e)
        {
            var reverse = _drawMatrix.Invert();
            var point = reverse.MapPoint(new SKPoint(e.Location.X, e.Location.Y));
            var x = (int)Math.Floor(point.X / 150);
            var y = (int)Math.Floor(point.Y / 150);

            if (x >= 0 && x < 10 && y >= 0 && y < 10)
                if (singleX == x && singleY == y)
                {
                    singleX = singleY = -1;
                    singleAnimation.Stop();
                }
                else
                {
                    singleX = x;
                    singleY = y;
                    singleAnimation.Start();
                }

            NeedsRedraw = true;
        }

        void OnDoubleTapped(object sender, TapEventArgs e)
        {
            var reverse = _drawMatrix.Invert();
            var point = reverse.MapPoint(new SKPoint(e.Location.X, e.Location.Y));
            var x = (int)(point.X / 150);
            var y = (int)(point.Y / 150);

            if (x >= 0 && x < 10 && y >= 0 && y < 10)
                if (doubleX == x && doubleY == y)
                    doubleX = doubleY = -1;
                else
                {
                    doubleX = x;
                    doubleY = y;
                }

            NeedsRedraw = true;
        }

        void OnLongPressing(object sender, TapEventArgs e)
        {
            var reverse = _drawMatrix.Invert();
            var point = reverse.MapPoint(new SKPoint(e.Location.X, e.Location.Y));
            var x = (int)(point.X / 150);
            var y = (int)(point.Y / 150);

            if (x >= 0 && x < 10 && y >= 0 && y < 10)
                if (longX == x && longY == y)
                    longX = longY = -1;
                else
                {
                    longX = x;
                    longY = y;
                }

            NeedsRedraw = true;
        }

        void OnWheelChanged(object sender, WheelChangedEventArgs e)
        {
            var scale = (float)(e.Delta > 0 ? Math.Pow(1.1, e.Delta / 120) : Math.Pow(0.9, -e.Delta/120));
            _drawMatrix = _drawMatrix.PostConcat(SKMatrix.CreateTranslation(-e.Location.X, -e.Location.Y));
            _drawMatrix = _drawMatrix.PostConcat(SKMatrix.CreateScale(scale, scale));
            _drawMatrix = _drawMatrix.PostConcat(SKMatrix.CreateTranslation(e.Location.X, e.Location.Y));

            NeedsRedraw = true;
        }

        void OnTimerCallback(object state)
        {
            // Called each 16 ms
            if (NeedsRedraw || Animation.UpdateAnimations())
                Redraw();
        }

        AnimationEntry CreateSwipeTranslationAnimationEntry(float velocityX, float velocityY, float maxDuration)
        {
            var magnitudeOfV = Math.Sqrt((velocityX * velocityX) + (velocityY * velocityY));

            var animateMillis = magnitudeOfV / 10;

            if (magnitudeOfV < 100 || animateMillis < 16)
                return null;

            if (animateMillis > maxDuration)
                animateMillis = maxDuration;

            AnimationEntry entry;

            entry = new AnimationEntry(
                start: (velocityX, velocityY),
                end: (0d, 0d),
                animationStart: 0,
                animationEnd: 1,
                easing: Easing.SinIn,
                tick: SwipeTick,
                final: null
            );
            return entry;
        }

        void SwipeTick(AnimationEntry entry, double value)
        {
            var timeAmount = TimeSpan.FromMilliseconds(16).TotalSeconds; // 16 / 1000d; // 16 milli

            (float velocityX, float velocityY) = ((float, float))entry.Start;

            var xMovement = velocityX * (1d - entry.Easing.Ease(value)) * timeAmount;
            var yMovement = velocityY * (1d - entry.Easing.Ease(value)) * timeAmount;

            if (xMovement.IsNanOrInfOrZero())
                xMovement = 0;
            if (yMovement.IsNanOrInfOrZero())
                yMovement = 0;

            if (xMovement == 0 && yMovement == 0)
                return;

            _drawMatrix = _drawMatrix.PostConcat(SKMatrix.CreateTranslation((float)xMovement, (float)yMovement));
        }

        #region DEMO

        void CreateColorsForDemo()
        {
            Random rand = new Random();

            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 10; j++)
                    colors[i, j] = new SKColor((byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255));
        }

        void DrawForDemo(SKCanvas canvas, SKPaint paint)
        {
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
                    canvas.DrawRect(150 * i, 150 * j, 150, 150, paint);

                    paint.Color = colors[i, j];
                    canvas.DrawRect(150 * i + 25, 150 * j + 25, 100, 100, paint);
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
