using Sensors.Dht;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Temperature
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int PIN = 17;

        private IDht dht = null;
        private GpioPin pin = null;        
        private DispatcherTimer timer = new DispatcherTimer();

        public MainPage()
        {
            this.InitializeComponent();

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += _timer_Tick;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var controller = GpioController.GetDefault();

            if (controller != null)
            {
                pin = controller.OpenPin(PIN, GpioSharingMode.Exclusive);
                dht = new Dht22(pin, GpioPinDriveMode.Input);

                timer.Start();
            }
        }

        private async void _timer_Tick(object sender, object e)
        {            
            var reading = await dht.GetReadingAsync().AsTask();   
            
            if(reading.IsValid)
            {
                Debug.WriteLine($"{DateTime.Now} - Temperature: {reading.Temperature}, Humidity: {reading.Humidity}");
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            timer.Stop();
            pin?.Dispose();
            pin = null;            
            dht = null;
        }
    }
}
