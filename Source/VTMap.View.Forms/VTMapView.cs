using SkiaSharp.Views.Forms;
using System;
using VTMap.Core.Events;
using VTMap.View.Forms.Extensions;
using Xamarin.Forms;

namespace VTMap.View
{
    public partial class VTMapView : ContentView //RelativeLayout
    {
        public static bool UseGPU = false;

        SKGLView _glView;
        SKCanvasView _canvasView;
        bool _sizeChanged = false;
        Action _invalidate;

        public VTMapView()
        {
            TouchEventHandler = new TouchEventHandler();

            if (UseGPU)
            {
                // Use GPU backend
                _glView = new SKGLView();
                _glView.HasRenderLoop = true;
                // Events
                _glView.EnableTouchEvents = true;
                _glView.Touch += OnTouch;
                _glView.PaintSurface += OnGLPaintSurface;
                _invalidate = () => _glView.InvalidateSurface();
            }
            else
            {
                // Use CPU backend
                _canvasView = new SKCanvasView();
                // Events
                _canvasView.EnableTouchEvents = true;
                _canvasView.Touch += OnTouch;
                _canvasView.PaintSurface += OnPaintSurface;
                _invalidate = () => _canvasView.InvalidateSurface();
            }

            Xamarin.Forms.View view;

            if (UseGPU)
                view = _glView;
            else
                view = _canvasView;

            view.SizeChanged += OnSizeChanged;

            Content = view;

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
            if (_sizeChanged)
            {
                _sizeChanged = false;
                InternalSizeChanged(_glView.CanvasSize.Width, _glView.CanvasSize.Height);
            }

            var canvas = args.Surface.Canvas;

            Draw(canvas);
        }

        void OnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            if (_sizeChanged)
            {
                _sizeChanged = false;
                InternalSizeChanged(_canvasView.CanvasSize.Width, _canvasView.CanvasSize.Height);
            }

            var canvas = args.Surface.Canvas;

            Draw(canvas);
        }

        void OnSizeChanged(object sender, EventArgs e)
        {
            _sizeChanged = true;
        }

        void Invalidate()
        {
            RunOnMainThread(() =>
            {
                _invalidate();
            });
        }

        void RunOnMainThread(Action action)
        {
            Device.BeginInvokeOnMainThread(() => action());
        }
    }
}
