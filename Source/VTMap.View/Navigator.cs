using System;
using VTMap.Core.Extensions;
using VTMap.Core.Primitives;
using VTMap.Core.Utilities;

namespace VTMap.View
{
    public class Navigator
    {
        Viewport _viewport;
        float _rotation;
        // Animations
        Animation _swipeAnimation;
        Animation _moveAnimation;
        Animation _rotateAnimation;
        Animation _scaleAnimation;

        public Navigator(Viewport viewport)
        {
            _viewport = viewport;
            _rotation = _viewport.Rotation;
            // Create animation for swipe gestures
            _swipeAnimation = new Animation(500);
        }

        // Value for rotation before rotation of viewport starts
        public float UnSnapRotation { get; set; } = 10.0f;

        // Value for rotation before rotation of viewport stops
        public float ReSnapRotation { get; set; } = 5.0f;

        public void CancelAnimations()
        {
            if (_swipeAnimation.IsRunning)
                _swipeAnimation.Stop(false);

            if (_rotateAnimation != null && _rotateAnimation.IsRunning)
                _rotateAnimation.Stop(false);
            if (_scaleAnimation != null && _scaleAnimation.IsRunning)
                _scaleAnimation.Stop(false);
        }

        public void MoveBy(float x, float y)
        {
            InternalMoveBy(x, y);
        }

        public void MoveTo(Core.Point newCenter, long duration = 0)
        {
            if (_moveAnimation != null && _moveAnimation.IsRunning)
                _moveAnimation.Stop(false);

            if (duration == 0)
            {
                InternalMoveBy(newCenter.X - _viewport.Center.X, newCenter.Y - _viewport.Center.Y);
                return;
            }

            // Animate movement
            _moveAnimation = new Animation(duration);
            _moveAnimation.Entries.Add(new AnimationEntry(
                start: _viewport.Center,
                end: newCenter,
                easing: Easing.Linear,
                tick: (entry, value) => {
                    var next = (Core.Point)entry.Start + ((Core.Point)entry.End - (Core.Point)entry.Start) * (float)value;
                    _viewport.Center = next;
                },
                final: (entry) => {
                    var next = (Core.Point)entry.End;
                    _viewport.Center = next;
                }));
            _moveAnimation.Start();
        }

        void InternalMoveBy(float x, float y)
        {
            var viewX = x;
            var viewY = y;

            if (_viewport.Rotation != 0)
            {
                double rad = -_viewport.Rotation.ToRadians();
                double rcos = Math.Cos(rad);
                double rsin = Math.Sin(rad);
                viewX = (float)(x * rcos + y * rsin);
                viewY = (float)(x * -rsin + y * rcos);
            }

            _viewport.Center = new Core.Point(_viewport.Center.X - 2 * viewX / _viewport.TileScaleFactor, _viewport.Center.Y + 2 * viewY / _viewport.TileScaleFactor);
        }

        /// <summary>
        /// Rotate Viewport by a relative angle around pivot screen point. 
        /// It respects UnSnapRotation and ReSnapRotation.
        /// </summary>
        /// <param name="newRotation">Relative angle in degrees</param>
        /// <param name="pivotScreen">Screen point to use as center for rotation</param>
        public void RotateBy(float degrees, Core.Point pivotScreen = null)
        {
            if (_rotateAnimation != null && _rotateAnimation.IsRunning)
                _rotateAnimation.Stop(false);

            _rotation -= degrees;

            if (_viewport.Rotation == 0 && UnSnapRotation > Math.Abs(_rotation))
                return;
            else if (_viewport.Rotation == 0 && UnSnapRotation <= Math.Abs(_rotation))
                degrees = _rotation;
            else if (_viewport.Rotation != 0 && ReSnapRotation > Math.Abs(_rotation))
                _rotation = 0;
             
            _rotation += degrees;

            InternalRotateBy(degrees, pivotScreen);
        }

        /// <summary>
        /// Rotate Viewport to a new angle around pivot screen point using animation with duration
        /// </summary>
        /// <param name="newRotation">New rotation angle in degrees</param>
        /// <param name="pivotScreen">Screen point to use as center for rotation</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        public void RotateTo(float newRotation, Core.Point pivotScreen = null, long duration = 0)
        {
            if (_rotateAnimation != null && _rotateAnimation.IsRunning)
                _rotateAnimation.Stop(false);

            var delta = (newRotation - _viewport.Rotation).ClampToDegree();

            if (duration == 0)
            {
                InternalRotateBy(delta, pivotScreen);
                return;
            }

            // Animate rotation
            _rotateAnimation = new Animation(duration);
            _rotateAnimation.Entries.Add(new AnimationEntry(
                start: _viewport.Rotation,
                end: newRotation,
                easing: Easing.Linear,
                tick: (entry, value) => { 
                    InternalRotateBy(_viewport.Rotation - ((float)entry.Start + delta * (float)value), pivotScreen); 
                },
                final: (entry) => {
                    InternalRotateBy(_viewport.Rotation - ((float)entry.Start + delta), pivotScreen);
                }));
            _rotateAnimation.Start();
        }

        void InternalRotateBy(float degrees, Core.Point pivotScreen = null)
        {
            _rotation -= degrees;

            var pivotCenter = pivotScreen is null ? _viewport.Center : _viewport.FromScreenToView(pivotScreen); // - SKFontStyleWidth * _mapView(_viewport.ScreenToViewMatrix.MapPoint(new SKPoint(pivotScreen.X, pivotScreen.Y)).ToPoint();
            var newCenter = new Core.Point(0, 0);
            var cosRotation = Math.Cos(-degrees.ToRadians());
            var sinRotation = Math.Sin(-degrees.ToRadians());

            newCenter.X = (float)(pivotCenter.X + cosRotation * (_viewport.Center.X - pivotCenter.X) - sinRotation * (_viewport.Center.Y - pivotCenter.Y));
            newCenter.Y = (float)(pivotCenter.Y + sinRotation * (_viewport.Center.X - pivotCenter.X) + cosRotation * (_viewport.Center.Y - pivotCenter.Y));

            _viewport.Rotation = _rotation;
            _viewport.Center = newCenter;

            _rotation = _viewport.Rotation;
        }

        public void ScaleBy(float scale, Core.Point pivotScreen = null)
        {
            if (_scaleAnimation != null && _scaleAnimation.IsRunning)
                _scaleAnimation.Stop(false);

            InternalScaleBy(scale, pivotScreen);
        }

        public void ScaleTo(float newScale, Core.Point pivotScreen = null, long duration = 0)
        {
            if (_scaleAnimation != null && _scaleAnimation.IsRunning)
                _scaleAnimation.Stop(false);

            if (duration == 0)
            {
                InternalScaleBy(newScale / _viewport.Scale, pivotScreen);
                return;
            }

            var delta = newScale - _viewport.Scale;

            // Animate scaling
            _scaleAnimation = new Animation(duration);
            _scaleAnimation.Entries.Add(new AnimationEntry(
                start: _viewport.Scale,
                end: newScale,
                easing: Easing.Linear,
                tick: (entry, value) => {
                    InternalScaleBy(((float)entry.Start + delta * (float)value) / _viewport.Scale, pivotScreen);
                },
                final: (entry) => {
                    InternalScaleBy(((float)entry.Start + delta) / _viewport.Scale, pivotScreen);
                }));
            _scaleAnimation.Start();
        }

        void InternalScaleBy(float scale, Core.Point pivotScreen = null)
        {
            // Save pivot point in view coordinate system
            var pivotView = pivotScreen is null ? _viewport.Center : _viewport.FromScreenToView(pivotScreen);
            var newCenter = pivotView + (_viewport.Center - pivotView) / scale;
            // Set new scale factor
            _viewport.Scale *= scale;
            _viewport.Center = newCenter;
        }

        public void SwipeWith(Vector velocity, long duration)
        {
            CancelAnimations();
            _swipeAnimation.Entries.Clear();
            if (velocity.X != 0 || velocity.Y != 0)
            {
                _swipeAnimation.Entries.Add(CreateSwipeTranslationAnimationEntry(velocity.X, velocity.Y, duration));
            }
            _swipeAnimation.Start();
        }

        AnimationEntry CreateSwipeTranslationAnimationEntry(float velocityX, float velocityY, float maxDuration)
        {
            var magnitudeOfV = Math.Sqrt((velocityX * velocityX) + (velocityY * velocityY));

            var animateMillis = magnitudeOfV / 10;

            if (magnitudeOfV < 100 || animateMillis < 16)
                return null;

            if (animateMillis > maxDuration)
                animateMillis = maxDuration;

            _swipeAnimation.Duration = (long)animateMillis;

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

            MoveBy((float)xMovement, (float)yMovement);
        }
    }
}
