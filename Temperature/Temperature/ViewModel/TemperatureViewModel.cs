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

        public TemperatureViewModel()
        {

        }        
    }
}
