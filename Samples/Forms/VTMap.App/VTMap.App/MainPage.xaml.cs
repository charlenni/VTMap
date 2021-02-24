using System;
using VTMap.View.Layers;
using Xamarin.Forms;

namespace VTMap.App
{
    public partial class MainPage : ContentPage
    {
        Random rand = new Random();

        public MainPage()
        {
            InitializeComponent();

            mapView.Map.Layers.Add(new CrossLayer());
            mapView.Map.Layers.Add(new TileIdLayer());

            mapView.Viewport.PropertyChanged += (s,e) => {
                Device.BeginInvokeOnMainThread(() =>
                {
                    var tiles = mapView.Viewport.Tiles;
                    labelCenter.Text = $"Center {mapView.Viewport.Center.X:0.000000}/{mapView.Viewport.Center.Y:0.000000}";// $"Box {mapView.Viewport.BoundingBox}"; // $"Center {mapView.Viewport.Center.X:0.000000}/{mapView.Viewport.Center.Y:0.000000}";
                    labelScale.Text = $"Scale {mapView.Viewport.Scale:#0.0000} / ZoomLevel {mapView.Viewport.ZoomLevel:0.0} / Zoom {mapView.Viewport.Zoom:#0.0000} / ZoomScale {mapView.Viewport.ZoomScale:#0.0000}";
                });
            };
            mapView.TouchEventHandler.TouchMove += (s, e) => {
                Device.BeginInvokeOnMainThread(() =>
                {
                    labelPosition.Text = $"Map {mapView.Viewport.FromScreenToView(e.Location).X}/{mapView.Viewport.FromScreenToView(e.Location).Y}";
                });
            };

            buttonCenter.Command = new Command(() =>
            {
                mapView.Navigator.MoveTo(Core.Point.Zero, 2000);
                mapView.Navigator.RotateTo(0f, null, 2000);
                mapView.Navigator.ScaleTo(1f, null, 2000);
            });
            buttonRotate.Command = new Command(() => { mapView.Navigator.RotateTo(mapView.Viewport.Rotation+10, null, 1000); /*mapView.Viewport.Roll += 2;*/ });  // RotateMap());
            buttonZoomIn.Command = new Command(() => mapView.Navigator.ScaleTo(mapView.Viewport.Scale * 2f, null, 1000));
            buttonZoomOut.Command = new Command(() => mapView.Navigator.ScaleTo(mapView.Viewport.Scale / 2f, null, 1000));
        }

        public void RotateMap()
        {
            var angle = mapView.Viewport.Rotation - 30; // -180.0f + 360.0f * (float)rand.NextDouble();
            mapView.Navigator.RotateTo(angle, null, 300);
        }
    }
}
