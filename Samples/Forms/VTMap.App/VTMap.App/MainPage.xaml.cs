using System;
using Xamarin.Forms;

namespace VTMap.App
{
    public partial class MainPage : ContentPage
    {
        Random rand = new Random();

        public MainPage()
        {
            InitializeComponent();

            mapView.Viewport.PropertyChanged += (s,e) => {
                Device.BeginInvokeOnMainThread(() =>
                {
                    labelCenter.Text = $"Center {mapView.Viewport.Center.X}/{mapView.Viewport.Center.Y}";
                    labelScale.Text = $"Scale {mapView.Viewport.Scale}";
                });
            };
            mapView.TouchEventHandler.TouchMove += (s, e) => {
                Device.BeginInvokeOnMainThread(() =>
                {
                    labelPosition.Text = $"Position Screen {e.Location.X}/{e.Location.Y} and Map {mapView.Viewport.FromScreenToView(e.Location).X}/{mapView.Viewport.FromScreenToView(e.Location).Y}";
                });
            };

            buttonRotate.Command = new Command(() => RotateMap());
            buttonZoomIn.Command = new Command(() => mapView.Navigator.ScaleTo(mapView.Viewport.Scale * 2f, null, 300));
            buttonZoomOut.Command = new Command(() => mapView.Navigator.ScaleTo(mapView.Viewport.Scale / 2f, null, 300));
        }

        public void RotateMap()
        {
            var angle = mapView.Viewport.Rotation - 30; // -180.0f + 360.0f * (float)rand.NextDouble();
            mapView.Navigator.RotateTo(angle, null, 300);
        }
    }
}
