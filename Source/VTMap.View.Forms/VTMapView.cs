using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using VTMap.Core.Events;
using VTMap.View.Forms.Extensions;
using Xamarin.Forms;

namespace VTMap.View
{
    public partial class VTMapView : RelativeLayout
    {
        public static bool UseGPU = true;

        SKGLView glView;
        SKCanvasView canView;

        public VTMapView()
        {
            TouchEventHandler = new TouchEventHandler();

            if (UseGPU)
            {
                glView = new SKGLView();
                glView.HasRenderLoop = true;
                // Events
                glView.EnableTouchEvents = true;
                glView.Touch += OnTouch;
                glView.PaintSurface += OnGLPaintSurface;
            }
            else
            {
                canView = new SKCanvasView();
                // Events
                canView.EnableTouchEvents = true;
                canView.Touch += OnTouch;
                canView.PaintSurface += OnPaintSurface;
            }

            Xamarin.Forms.View view;

            if (UseGPU)
                view = glView;
            else
                view = canView;

            view.SizeChanged += OnSizeChanged;

            Children.Add(view,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent((parent) => parent.Width),
                Constraint.RelativeToParent((parent) => parent.Height));

            InternalInit();
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

        void OnGLPaintSurface(object sender, SKPaintGLSurfaceEventArgs args)
        {
            var canvas = args.Surface.Canvas;

            Draw(canvas);
        }

        void OnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            var canvas = args.Surface.Canvas;

            Draw(canvas);
        }

        void Invalidate()
        {
            RunOnMainThread(() =>
            {
                if (UseGPU)
                    glView.InvalidateSurface();
                else
                    canView.InvalidateSurface();
            });
        }

        void RunOnMainThread(Action action)
        {
            Device.BeginInvokeOnMainThread(() => action());
        }

        SKSize GetCanvasSize()
        {
            return UseGPU == true ? glView.CanvasSize : canView.CanvasSize;
        }
        float GetPixelDensity()
        {
            if (Width <= 0) 
                return 0;

            return (float)(UseGPU == true ? glView.CanvasSize.Width / Width : canView.CanvasSize.Width / Width);
        }
    }
}
