using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.ServiceBus.Messaging;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;
using TemperatureWeb.Models;
using System.Web.Hosting;
using TemperatureWeb.ViewModels;
using TemperatureWeb.SignalRHub;

namespace TemperatureWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private const string connectionString = "HostName=MoMoIoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=D2WedzOpGOAmF1vXZRhuVJ4JhweRnB2+PraXzLJBnsk=";
        private const string iotHubD2cEndpoint = "messages/events";

        private EventHubClient eventHubClient;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            HostingEnvironment.QueueBackgroundWorkItem((ct) => ReceiveMessages(ct));            
        }

        private void ReceiveMessages(CancellationToken token)
        {
            Debug.WriteLine("Receive messages.");

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);
            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

            var tasks = new List<Task>();
            foreach (string partition in d2cPartitions)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, token));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);

            while (true)
            {
                if (ct.IsCancellationRequested) break;
                EventData eventData = await eventHubReceiver.ReceiveAsync();
                if (eventData == null) continue;

                var data = Encoding.UTF8.GetString(eventData.GetBytes());
                Debug.WriteLine("Message received. Partition: {0} Data: '{1}'", partition, data);

                var temperature = JsonConvert.DeserializeObject<TemperatureData>(data);
                temperature.PartitionId = partition;

                var hub = new TemperatureHub();
                await hub.NotifyClient(temperature);
            }
        }
    }
}
