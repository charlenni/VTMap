using Serilog;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VTMap.App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            //.WriteTo.Console()
                            //.WriteTo.File("logs\\my_log.log", rollingInterval: RollingInterval.Day)
                            .CreateLogger();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
