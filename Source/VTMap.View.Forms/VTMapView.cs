using SkiaSharp.Views.Forms;
using System;
using VTMap.Core.Events;
using VTMap.View.Forms.Extensions;
using Xamarin.Forms;

namespace VTMap.View
{
    public partial class VTMapView : ContentView
    {
        public static bool UseGPU = true;

        SKGLView _glView;
        SKCanvasView _canvasView;
        Action _invalidate;

        public VTMapView()
        {
            TouchEventHandler = new TouchEventHandler();

            Xamarin.Forms.View view;

            if (UseGPU)
            {
                // Use GPU backend
                _glView = new SKGLView
                {
                    HasRenderLoop = true,
                    EnableTouchEvents = true,
                };
                // Events
                _glView.Touch += OnTouch;
                _glView.PaintSurface += OnGLPaintSurface;
                _invalidate = () => { _glView.InvalidateSurface(); };
                view = _glView;
            }
            else
            {
                // Use CPU backend
                _canvasView = new SKCanvasView
                {
                    EnableTouchEvents = true,
                };
                // Events
                _canvasView.Touch += OnTouch;
                _canvasView.PaintSurface += OnPaintSurface;
                _invalidate = () => { RunOnMainThread(() => _canvasView.InvalidateSurface()); };
                view = _canvasView;
            }

            view.SizeChanged += OnSizeChanged;

            Content = view;

            InternalInit();
        }

        void OnGLPaintSurface(object sender, SKPaintGLSurfaceEventArgs args)
        {
            Draw(args.Surface.Canvas);
        }

        void OnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            Draw(args.Surface.Canvas);
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

        void OnSizeChanged(object sender, EventArgs e)
        {
            var density = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;
            InternalSizeChanged((float)(Width * density), (float)(Height * density));
        }

        void Invalidate()
        {
            _invalidate();
        }

        void RunOnMainThread(Action action)
        {
            Device.BeginInvokeOnMainThread(() => action());
        }
    }
}
