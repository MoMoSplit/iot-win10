using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using TemperatureWeb.Models;
using TemperatureWeb.ViewModels;
using System.Data.Entity;

namespace TemperatureWeb.SignalRHub
{
    [Authorize()]
    public class TemperatureHub : Hub
    {
        public static readonly ConnectionMapping<string> Connections = new ConnectionMapping<string>();

        public async Task NotifyClient(TemperatureData temperature)
        {
            using (var context = IoTDbContext.Create())
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Devices.Any(d => d.Key == temperature.DeviceId));

                if(Connections.UserExists(user.Id))
                {
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<TemperatureHub>();

                    var connId = Connections.GetConnections(user.Id).ToList();
                    hubContext.Clients.Clients(connId).receiveTemperature(temperature);
                }
            }                
        }

        public override Task OnConnected()
        {
            var userId = Context.User.Identity.GetUserId();
            Connections.Add(userId, Context.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userId = Context.User.Identity.GetUserId();
            Connections.Remove(userId, Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var userId = Context.User.Identity.GetUserId();
            if (!Connections.GetConnections(userId).Contains(Context.ConnectionId))
            {
                Connections.Add(userId, Context.ConnectionId);
            }

            return base.OnReconnected();
        }
    }
}