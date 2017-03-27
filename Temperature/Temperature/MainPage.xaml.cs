using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using Newtonsoft.Json;
using Sensors.Dht;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Temperature.ViewModel;
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
        private RegistryManager registryManager;

        private const int SENSOR_PIN_NUM = 17;
        private const int LED_PIN_NUM = 2;
        private const int RELAY_PIN_NUM = 3;

        private IDht dht = null;

        private GpioPin sensorPin = null;
        private GpioPin ledPin = null;
        private GpioPin isRelayPin = null;

        private DispatcherTimer timer = new DispatcherTimer();

        private List<int> retries = new List<int>();
        private bool relayOn = false;

        public MainPage()
        {
            this.InitializeComponent();

            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += _timer_Tick;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var controller = GpioController.GetDefault();
            if (controller == null)
                return;

            sensorPin = controller.OpenPin(SENSOR_PIN_NUM, GpioSharingMode.Exclusive);
            dht = new Dht22(sensorPin, GpioPinDriveMode.Input);

            ledPin = controller.OpenPin(LED_PIN_NUM, GpioSharingMode.Exclusive);
            ledPin.SetDriveMode(GpioPinDriveMode.Output);

            isRelayPin = controller.OpenPin(RELAY_PIN_NUM, GpioSharingMode.Exclusive);
            isRelayPin.SetDriveMode(GpioPinDriveMode.Output);

            registryManager = RegistryManager.CreateFromConnectionString(Config.ConnectionString);
                        
            ReceiveDataFromAzure();

            timer.Start();
        }

        private async void _timer_Tick(object sender, object e)
        {
            var vm = this.DataContext as TemperatureViewModel;
            var dhtData = await dht.GetReadingAsync().AsTask();   
            
            if(dhtData.IsValid)
            {
                retries.Add(dhtData.RetryCount);

                vm.Humidity = dhtData.Humidity;
                vm.Temperature = dhtData.Temperature;
                vm.AverageRetries = retries.Average();                
                vm.TotalSuccess++;

                Debug.WriteLine($"{DateTime.Now} - Temperature: {dhtData.Temperature}, Humidity: {dhtData.Humidity}");

                ShouldITurnOnAirCondition(dhtData);

                await SendDataToAzureAsync(dhtData);
                await Blink();
            }
                        
            vm.TotalAttempts++;

            vm.PercentSuccess = 100d * ((double)vm.TotalSuccess / (double)vm.TotalAttempts);
            vm.LastUpdated = DateTime.Now;
        }        

        private async Task SendDataToAzureAsync(DhtReading dhtData)
        {
            var cloudData = new
            {
                DeviceId = Config.deviceId,
                Temperature = dhtData.Temperature,
                Humidity = dhtData.Humidity,
                Time = DateTime.Now,
                RetryCount = dhtData.RetryCount
            };

            var messageString = JsonConvert.SerializeObject(cloudData);
            var message = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(messageString));            

            var deviceClient = DeviceClient.CreateFromConnectionString(Config.deviceConnectionString);

            await deviceClient.SendEventAsync(message);
            Debug.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
        }

        public async void ReceiveDataFromAzure()
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(Config.deviceConnectionString);   
            
            while (true)
            {
                var receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage != null)
                {
                    var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());                    
                    await deviceClient.CompleteAsync(receivedMessage);

                    switch (messageData)
                    {
                        case "TurnOn":
                            TurnOnRelay();
                            break;
                        case "TurnOff":
                            TurnOffRelay();
                            break;
                    }
                }
            }
        }        

        private void ShouldITurnOnAirCondition(DhtReading dhtData)
        {
            if ((dhtData.Temperature >= 24 || dhtData.Humidity >= 75) && !relayOn)
                TurnOnRelay();            
        }

        private async Task Blink()
        {
            ledPin.Write(GpioPinValue.Low);
            await Task.Delay(500);
            ledPin.Write(GpioPinValue.High);
        }

        private void TurnOffRelay()
        {
            lock(isRelayPin)
            {
                isRelayPin.Write(GpioPinValue.High);
                relayOn = false;
            }            
        }

        private void TurnOnRelay()
        {
            lock(isRelayPin)
            {
                isRelayPin.Write(GpioPinValue.Low);
                relayOn = true;
            }            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            timer.Stop();

            sensorPin?.Dispose();
            sensorPin = null;

            ledPin?.Dispose();
            ledPin = null;

            dht = null;
        }
    }
}
