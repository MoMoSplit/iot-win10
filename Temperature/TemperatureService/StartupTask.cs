using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Sensors.Dht;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml;
using Windows.Devices.Gpio;
using System.Diagnostics;
using System.Threading;
using Windows.System.Threading;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.IO;
using Restup.Webserver.Rest;
using Restup.Webserver.Http;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.Networking;
using Newtonsoft.Json;
using Windows.Networking.Connectivity;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace TemperatureService
{
    public sealed class StartupTask : IBackgroundTask
    {
        private IDht dht = null;
        private GpioPin pin = null;
        private ThreadPoolTimer timer;        

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            var controller = GpioController.GetDefault();

            if (controller == null)
                return;

            pin = controller.OpenPin(17, GpioSharingMode.Exclusive);
            dht = new Dht22(pin, GpioPinDriveMode.Input);

            timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromMilliseconds(2));            
        }

        private async void Timer_Tick(ThreadPoolTimer e)
        {            
            var reading = await dht.GetReadingAsync().AsTask();

            if (reading.IsValid)
            {
                Debug.WriteLine($"{DateTime.Now} - Temperature: {reading.Temperature}, Humidity: {reading.Humidity}");                
            }
        }        
    }
}
