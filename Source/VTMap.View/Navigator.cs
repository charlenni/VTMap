using SkiaSharp;
using System;
using VTMap.Core.Extensions;
using VTMap.Core.Primitives;
using VTMap.Core.Utilities;
using VTMap.Extensions;

namespace VTMap.View
{
    public class Navigator
    {
        Viewport _viewport;
        // Animations
        Animation _swipeAnimation;

        public Navigator(Viewport viewport)
        {
            _viewport = viewport;
            // Create animation for swipe gestures
            _swipeAnimation = new Animation(500);
        }

        public void Cancel()
        {
            if (_swipeAnimation.IsRunning)
            {
                _swipeAnimation.Stop();
            }
        }

        public void MoveByPixel(float x, float y)
        {
            var changeMatrix = SKMatrix.CreateTranslation(-x, -y);
            var newMatrix = _viewport.ScreenToViewMatrix.PreConcat(changeMatrix);
            var newCenter = newMatrix.MapPoint(new SKPoint(_viewport.Width * _viewport.PixelDensity / 2, _viewport.Height * _viewport.PixelDensity / 2));
            _viewport.Center = new Core.Point(newCenter.X, newCenter.Y);
        }

        public void RotateBy(float degrees)
        {
            RotateBy(_viewport.Bearing, new Core.Point(_viewport.Width / 2, _viewport.Height / 2));
        }

        public void RotateBy(float degrees, Core.Point pivot)
        {
            var rotationMatrix = SKMatrix.CreateRotationDegrees(-degrees, pivot.X, pivot.Y);
            var newMatrix = _viewport.ScreenToViewMatrix.PreConcat(rotationMatrix);
            var newCenter = newMatrix.MapPoint(new SKPoint(_viewport.Width * _viewport.PixelDensity / 2, _viewport.Height * _viewport.PixelDensity / 2));
            _viewport.Bearing -= degrees;
            _viewport.Center = new Core.Point(newCenter.X, newCenter.Y);
        }

        public void ScaleBy(float scale)
        {
            ScaleBy(scale, new Core.Point(_viewport.Width / 2, _viewport.Height / 2));
        }

        public void ScaleBy(float scale, Core.Point pivotScreen)
        {
            // Save pivot point in view coordinate system
            var pivotView = _viewport.ScreenToViewMatrix.MapPoint(new SKPoint(pivotScreen.X, pivotScreen.Y)).ToPoint();
            var newCenter = pivotView + (_viewport.Center - pivotView) / scale;
            // Set new scale factor
            _viewport.Scale *= scale;
            _viewport.Center = newCenter;
        }

        public void MoveRotateScaleBy(float x, float y, float degrees, float scale, Core.Point pivot)
        {
            var pivotX = pivot.X + x;
            var pivotY = pivot.Y + y;

            var changeMatrix = SKMatrix.CreateTranslation(-pivotX, -pivotY); // _viewport.Width / 2 - pivot.X, _viewport.Height / 2 - pivot.Y);
            changeMatrix = changeMatrix.PostConcat(SKMatrix.CreateRotationDegrees(-degrees));
            changeMatrix = changeMatrix.PostConcat(SKMatrix.CreateScale(scale, scale));
            changeMatrix = changeMatrix.PostConcat(SKMatrix.CreateTranslation(pivotX / scale, pivotY / scale)); // _viewport.Width / 2 - pivot.X, _viewport.Height / 2 - pivot.Y));
            var newMatrix = _viewport.ScreenToViewMatrix.PreConcat(changeMatrix);
            var newCenter = newMatrix.MapPoint(new SKPoint(_viewport.Width / 2, _viewport.Height / 2));
            _viewport.Bearing -= degrees;
            _viewport.Scale *= scale;
            _viewport.Center = new Core.Point(newCenter.X, newCenter.Y);
        }

        public void SwipeWith(Vector velocity, long duration)
        {
            Cancel();
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

            MoveByPixel((float)xMovement, (float)yMovement);
        }
    }
}
