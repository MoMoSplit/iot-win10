using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace TemperatureWeb.Models
{
    public class TemperatureEntity : TableEntity, IComparable
    {        
        public string deviceId { get; set; }
        public double temperature { get; set; }
        public double humidity { get; set; }
        public DateTime time { get; set; }
        public int retryCount { get; set; }

        public TemperatureEntity()
        {

        }

        public TemperatureEntity(string deviceId, DateTime time)
        {
            this.PartitionKey = deviceId;
            this.RowKey = time.ToString(CultureInfo.InvariantCulture);
        }

        public int CompareTo(object obj)
        {
            return this.CompareTo(((TemperatureEntity)obj).time);
        }
    }
}