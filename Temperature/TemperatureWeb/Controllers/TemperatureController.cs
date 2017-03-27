using Microsoft.AspNet.Identity;
using Microsoft.Azure.Devices;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TemperatureWeb.Models;

namespace TemperatureWeb.Controllers
{
    [Authorize]
    public class TemperatureController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Chart()
        {
            var userDeviceKey = await GetUserDeviceId();

            var storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();

            var table = tableClient.GetTableReference("Temperature");
            var query = new TableQuery<TemperatureEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userDeviceKey));

            var data = table.ExecuteQuery(query).Where(x => x.temperature != 0 && x.humidity != 0).ToList();

            return View(data);
        }        

        [HttpGet]
        public ActionResult Current()
        {            
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> GetLastHour()
        {
            var userDeviceKey = await GetUserDeviceId();

            var storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();

            var table = tableClient.GetTableReference("Temperature");
            var query = new TableQuery<TemperatureEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userDeviceKey));

            var data = table.ExecuteQuery(query)                   
                .Where(x => x.temperature != 0 && x.humidity != 0)
                .GroupBy(x => new { date = new DateTime(x.time.Year, x.time.Month, x.time.Day, x.time.Hour, 0, 0) })
                .OrderByDescending(x => x.Key.date)
                .Take(50)
                .Select(x => new
                {
                    time = x.Key.date,
                    temperature = Math.Round(x.Average(t => t.temperature), 2),
                    humidity = Math.Round(x.Average(h => h.humidity), 2)
                })
                .ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TurnOn()
        {
            await SendMessageToDevice("TurnOn");

            if(Request.IsAjaxRequest())
                return new HttpStatusCodeResult(HttpStatusCode.Created, "Ok");
            else
                return RedirectToAction("Current");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TurnOff()
        {
            await SendMessageToDevice("TurnOff");

            if (Request.IsAjaxRequest())
                return new HttpStatusCodeResult(HttpStatusCode.Created, "Ok");
            else
                return RedirectToAction("Current");
        }

        private async Task SendMessageToDevice(string message)
        {
            var userDeviceKey = await GetUserDeviceId();

            var serviceClient = ServiceClient.CreateFromConnectionString(Config.ConnectionString);
            var commandMessage = new Message(Encoding.UTF8.GetBytes(message));
            await serviceClient.SendAsync(userDeviceKey, commandMessage);
        }

        private async Task<string> GetUserDeviceId()
        {
            var userDeviceKey = string.Empty;
            var userId = User.Identity.GetUserId();

            using (var context = IoTDbContext.Create())
            {
                var device = await context.Devices.FirstOrDefaultAsync(d => d.ApplicationUserId == userId);
                userDeviceKey = device.Key;
            }

            return userDeviceKey;
        }
    }
}