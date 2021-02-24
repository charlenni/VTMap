using System;
using System.Collections.Generic;
using System.Threading;
using VTMap.Core.Enums;
using VTMap.Core.Utilities;

namespace VTMap.Core.Events
{
    /// <summary>
    /// Most of the used things found at
    /// https://docs.microsoft.com/de-de/xamarin/xamarin-forms/user-interface/graphics/skiasharp/transforms/touch
    /// https://github.com/OndrejKunc/SkiaScene/blob/master/source/SkiaScene/SkiaScene.TouchManipulation/TouchGestureRecognizer.cs
    /// </summary>
    public class TouchEventHandler : ITouchEventHandler
    {
        const float PI2 = (float)(2.0f * Math.PI);

        protected DateTime LastTapTime = DateTime.MinValue;
        protected DateTime LastDoubleTapTime = DateTime.MinValue;

        readonly Dictionary<long, TouchManipulationInfo> _touchDictionary = new Dictionary<long, TouchManipulationInfo>();
        readonly VelocityTracker _velocityTracker = new VelocityTracker();

        Point _firstTouchPoint;

        DateTime _lastLongTapTime = DateTime.MinValue;

        Timer _timerDoubleTap;
        Timer _timerLongPress;

        float _rotation = 0;
        float _scale = 1;

        /// <summary>
        /// Time in milliseconds until a second touch event is registered as double tap
        /// </summary>
        public TimeSpan DoubleTapDelay { get; set; } = TimeSpan.FromMilliseconds(320);

        /// <summary>
        /// Time in milliseconds until a touch event is resitered as long press
        /// </summary>
        public TimeSpan LongPressDelay { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// MaxMoveDistance is the number of pixel that distinguish between tap and pan
        /// </summary>
        public int MaxMoveDistance { get; set; } = 4;

        public event EventHandler<TouchEventArgs> TouchDown;
        public event EventHandler<TouchEventArgs> TouchUp;
        public event EventHandler<TouchEventArgs> TouchMove;
        public event EventHandler<TapEventArgs> Tapping;
        public event EventHandler<TapEventArgs> SingleTapped;
        public event EventHandler<TapEventArgs> DoubleTapped;
        public event EventHandler<TapEventArgs> LongPressing;
        public event EventHandler<TapEventArgs> LongPressed;
        public event EventHandler<PanEventArgs> Panning;
        public event EventHandler<PanEventArgs> Panned;
        public event EventHandler<PinchEventArgs> Pinching;
        public event EventHandler<PinchEventArgs> Pinched;
        public event EventHandler<SwipeEventArgs> Swiped;
        public event EventHandler<WheelChangedEventArgs> WheelChanged;

        public TouchEventHandler()
        { }

        public void Handle(TouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case TouchActionType.Entered:
                    break;
                case TouchActionType.Pressed:
                    if (HandleDown(e))
                        return;
                    SaveTouch(e);
                    if (_touchDictionary.Count == 1)
                    {
                        _lastLongTapTime = DateTime.Now;
                        CreateLongPressTimer(e);
                        _firstTouchPoint = e.Location;
                    }
                    else
                    {
                        // Another touch is register, so stop panning
                        TouchManipulationInfo[] infos = new TouchManipulationInfo[_touchDictionary.Count];
                        _touchDictionary.Values.CopyTo(infos, 0);
                        if (infos.Length == 1)
                        {
                            Point previousPoint = infos[0].PreviousPoint;
                            Point newPoint = infos[0].NewPoint;
                            // Did we moved some distance? If yes, then it was a pan event 
                            HandlePanned(new PanEventArgs(previousPoint, newPoint, TouchActionType.Pressed));
                        }
                        _rotation = 0;
                        _scale = 1;
                    }
                    break;
                case TouchActionType.Moved:
                    if (HandleMove(e))
                        return;
                    // Was there already a pressed action for this?
                    if (!_touchDictionary.ContainsKey(e.Id))
                    {
                        return;
                    }
                    TouchManipulationInfo info = _touchDictionary[e.Id];
                    info.NewPoint = e.Location;
                    // If moved to far from original point
                    if (!info.IsMoving && Distance(_firstTouchPoint, e.Location) > MaxMoveDistance)
                    {
                        info.IsMoving = true;
                        ClearLongPressTimer();
                    }
                    _velocityTracker.AddEvent(e.Id, e.Location, DateTime.Now.Ticks);
                    DetectPinchAndPanGestures(e.Id, e.ActionType);
                    info.PreviousPoint = info.NewPoint;
                    break;
                case TouchActionType.Released:
                    if (HandleUp(e))
                        return;
                    // Was there already a pressed action for this?
                    if (!_touchDictionary.ContainsKey(e.Id))
                    {
                        return;
                    }
                    ClearLongPressTimer();
                    _touchDictionary[e.Id].NewPoint = e.Location;
                    DetectTapGestures(e.Id);
                    DetectPinchAndPanGestures(e.Id, e.ActionType);
                    _touchDictionary.Remove(e.Id);
                    break;
                case TouchActionType.Cancelled:
                    if (!_touchDictionary.ContainsKey(e.Id))
                    {
                        return;
                    }
                    ClearLongPressTimer();
                    _touchDictionary.Remove(e.Id); 
                    break;
                case TouchActionType.Exited:
                    ClearLongPressTimer();
                    break;
                case TouchActionType.WheelChanged:
                    HandleWheelChanged(new WheelChangedEventArgs(e.Id, e.Location, e.WheelDelta, e.MouseButton));
                    break;
                default:
                    throw new NotImplementedException();
            }

            e.Handled = true;
        }

        public bool HandleDown(TouchEventArgs e)
        {
            TouchDown?.Invoke(this, e);

            return e.Handled;
        }

        public bool HandleUp(TouchEventArgs e)
        {
            TouchUp?.Invoke(this, e);

            return e.Handled;
        }

        public bool HandleMove(TouchEventArgs e)
        {
            TouchMove?.Invoke(this, e);

            return e.Handled;
        }

        // Unsure tapping event. Could become a double tap
        public void HandleTapping(TapEventArgs e)
        {
            Tapping?.Invoke(this, e);
        }

        // Certified single tap. No tap in special time range.
        public void HandleSingleTapped(TapEventArgs e)
        {
            SingleTapped?.Invoke(this, e);
        }

        public void HandleDoubleTapped(TapEventArgs e)
        {
            DoubleTapped?.Invoke(this, e);
        }

        // Still pressing
        public void HandleLongPressing(TapEventArgs e)
        {
            LongPressing?.Invoke(this, e);
        }

        public void HandleLongPressed(TapEventArgs e)
        {
            LongPressed?.Invoke(this, e);
        }

        // A finger came down and is moving on the screen.
        public void HandlePanning(PanEventArgs e)
        {
            Panning?.Invoke(this, e);
        }

        public void HandlePanned(PanEventArgs e)
        {
            Panned?.Invoke(this, e);
        }

        public void HandleSwiped(SwipeEventArgs e)
        {
            Swiped?.Invoke(this, e);
        }

        //  Two fingers hit the screen and are moving towards or away from each other
        public void HandlePinching(PinchEventArgs e)
        {
            Pinching?.Invoke(this, e);
        }

        public void HandlePinched(PinchEventArgs e)
        {
            Pinched?.Invoke(this, e);
        }

        private void HandleWheelChanged(WheelChangedEventArgs e)
        {
            WheelChanged?.Invoke(this, e);
        }

        private void DetectTapGestures(long id)
        {
            TouchManipulationInfo[] infos = new TouchManipulationInfo[_touchDictionary.Count];
            _touchDictionary.Values.CopyTo(infos, 0);
            // Only one finger taps are registered
            if (infos.Length != 1)
            {
                return;
            }
            Point point = infos[0].PreviousPoint;
            if (infos[0].IsMoving)
            {
                return;
            }
            var tapEventArgs = new TapEventArgs(id, point);

            var now = DateTime.Now;
            var lastTapTime = LastTapTime;
            LastTapTime = now;

            HandleTapping(tapEventArgs);

            if (now - lastTapTime < DoubleTapDelay)
            {
                HandleDoubleTapped(tapEventArgs);
                LastDoubleTapTime = now;
                LastTapTime = DateTime.MinValue; //Reset double tap timer
            }
            else if (now - _lastLongTapTime >= LongPressDelay)
            {
                HandleLongPressed(tapEventArgs);
            }
            else
            {
                CreateDoubleTapTimer(tapEventArgs);
            }
        }

        private void DetectPinchAndPanGestures(long id, TouchActionType touchActionType)
        {
            TouchManipulationInfo[] infos = new TouchManipulationInfo[_touchDictionary.Count];
            _touchDictionary.Values.CopyTo(infos, 0);

            if (infos.Length == 1)
            {
                Point previousPoint = infos[0].PreviousPoint;
                Point newPoint = infos[0].NewPoint;
                // Did we moved some distance? If yes, then it is a pan event 
                if (infos[0].IsMoving)
                {
                    if (touchActionType == TouchActionType.Moved)
                        HandlePanning(new PanEventArgs(previousPoint, newPoint, touchActionType));
                    else
                    {
                        var (velocityX, velocityY) = _velocityTracker.CalcVelocity(id, DateTime.Now.Ticks);
                        if (Math.Abs(velocityX) > 200 || Math.Abs(velocityY) > 200)
                            HandleSwiped(new SwipeEventArgs(newPoint, new Primitives.Vector(velocityX, velocityY), 0, 0, touchActionType));
                        else
                            HandlePanned(new PanEventArgs(previousPoint, newPoint, touchActionType));
                    }
                }
            }
            else if (infos.Length >= 2)
            {
                // Create old and new mid points
                Point prevMidPoint = PointBetween(infos[0].PreviousPoint, infos[1].PreviousPoint);
                Point newMidPoint = PointBetween(infos[0].NewPoint, infos[1].NewPoint);
                var translation = newMidPoint - prevMidPoint;
                // Which point is still at the same position
                int pivotIndex = infos[0].NewPoint == infos[0].PreviousPoint ? 0 : 1;
                Point pivotPoint = infos[pivotIndex].NewPoint;
                Point newPoint = infos[1 - pivotIndex].NewPoint;
                Point prevPoint = infos[1 - pivotIndex].PreviousPoint;
                // Calculate two vectors
                Point oldVector = prevPoint - pivotPoint;
                Point newVector = newPoint - pivotPoint;
                // Calc rotation
                float oldAngle = (float)Math.Atan2(oldVector.Y, oldVector.X);
                float newAngle = (float)Math.Atan2(newVector.Y, newVector.X);
                // Calc scaling
                // Effectively rotate the old vector
                float magnitudeRatio = Magnitude(oldVector) / Magnitude(newVector);
                oldVector.X = magnitudeRatio * newVector.X;
                oldVector.Y = magnitudeRatio * newVector.Y;
                // Isotropic scaling!
                float scale = Magnitude(newVector) / Magnitude(oldVector);
                // Calc new values
                _rotation += newAngle - oldAngle;
                while (_rotation > PI2)
                    _rotation -= PI2;
                while (_rotation < -PI2)
                    _rotation += PI2;
                _scale *= scale;
                // Pinching
                var args = new PinchEventArgs(prevPoint, newPoint, pivotPoint, newMidPoint, translation, (float)((newAngle - oldAngle)*180/Math.PI), scale, touchActionType);
                if (touchActionType == TouchActionType.Moved)
                    HandlePinching(args);
                else
                    HandlePinched(args);
                if (args.Handled)
                    return;
            }
        }

        private void SaveTouch(TouchEventArgs e)
        {
            var tmi = new TouchManipulationInfo
            {
                PreviousPoint = e.Location,
                NewPoint = e.Location,
            };

            if (_touchDictionary.ContainsKey(e.Id))
                _touchDictionary[e.Id] = tmi;
            else
                _touchDictionary.Add(e.Id, tmi);
        }

        private void CreateDoubleTapTimer(TapEventArgs tapEventArgs)
        {
            _timerDoubleTap = new Timer(_ =>
            {
                if (DateTime.Now - LastDoubleTapTime < DoubleTapDelay)
                {
                    return;
                }
                HandleSingleTapped(tapEventArgs);
            }, null, DoubleTapDelay.Milliseconds, Timeout.Infinite);
        }

        private void CreateLongPressTimer(TouchEventArgs e)
        {
            _timerLongPress = new Timer(_ =>
            {
                if (DateTime.Now - _lastLongTapTime < LongPressDelay)
                {
                    return;
                }
                HandleLongPressing(new TapEventArgs(e.Id, e.Location));
            }, null, LongPressDelay.Milliseconds, Timeout.Infinite);
        }

        private void ClearLongPressTimer()
        {
            if (_timerLongPress == null)
                return;

            _timerLongPress.Change(Timeout.Infinite, Timeout.Infinite);
            _timerLongPress = null;
        }

        private float Distance(Point a, Point b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        private float Magnitude(Point a)
        {
            return (float)Math.Sqrt(Math.Pow(a.X, 2) + Math.Pow(a.Y, 2));
        }

        private Point PointBetween(Point a, Point b)
        {
            var midX = a.X + (b.X - a.X) / 2;
            var midY = a.Y + (b.Y - a.Y) / 2;

            return new Point(midX, midY);
        }
    }
}
