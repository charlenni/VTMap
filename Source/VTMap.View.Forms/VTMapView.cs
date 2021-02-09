using SkiaSharp.Views.Forms;
using System;
using VTMap.Core.Events;
using VTMap.View.Forms.Extensions;
using Xamarin.Forms;

namespace VTMap.View
{
    public partial class VTMapView : SKGLView
    {
        public VTMapView()
        {
            var dpi = this.
            TouchEventHandler = new TouchEventHandler();

            InternalInit();

            HasRenderLoop = true;

            // Events
            EnableTouchEvents = true;
            Touch += OnTouch;
            SizeChanged += OnSizeChanged;
        }

        void OnTouch(object sender, SKTouchEventArgs e)
        {
            // Convert SkiaSharp touch event args to VTMap touch event args
            var args = e.ToTouchEventArgs();
            // Handle event
            TouchEventHandler?.Handle(args);
            // Save flag for later use
            e.Handled = args.Handled;
        }

        protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs args)
        {
            // call the base method
            base.OnPaintSurface(args);

            var canvas = args.Surface.Canvas;

            Draw(canvas);
        }

        void RunOnMainThread(Action action)
        {
            Device.BeginInvokeOnMainThread(() => action());
        }

        float GetPixelDensity()
        {
            if (Width <= 0) 
                return 0;

            return (float)(CanvasSize.Width / Width);
        }
    }
}
