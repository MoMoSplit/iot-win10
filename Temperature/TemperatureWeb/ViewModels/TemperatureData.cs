using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TemperatureWeb.ViewModels
{
    public class TemperatureData
    {
        public string DeviceId { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public DateTime Time { get; set; }
        public int RetryCount { get; set; }
        public string PartitionId { get; set; }

    }
}