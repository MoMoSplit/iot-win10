using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temperature.ViewModel
{
    public class TemperatureViewModel : NotifyBase
    {
        public double Temperature
        {
            get => Get<double>();
            set => Set(value);
        }

        public double Humidity
        {
            get => Get<double>();
            set => Set(value);
        }

        public DateTime LastUpdated
        {
            get => Get<DateTime>();
            set => Set(value);
        }

        public int TotalAttempts
        {
            get => Get<int>();
            set => Set(value);
        }

        public int TotalSuccess
        {
            get => Get<int>();
            set => Set(value);
        }

        public double AverageRetries
        {
            get => Get<double>();
            set => Set(value);
        }

        public double PercentSuccess
        {
            get => Get<double>();
            set => Set(value);
        }

        public double SuccessRate
        {
            get => Get<double>();
            set => Set(value);
        }        

        public TemperatureViewModel()
        {

        }        
    }
}
