using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temperature
{
    public class Config
    {
        public const string ConnectionString = "HostName=MoMoIoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=D2WedzOpGOAmF1vXZRhuVJ4JhweRnB2+PraXzLJBnsk=";
        public const string deviceConnectionString = "HostName=MoMoIoT.azure-devices.net;DeviceId=FESBTemperature;SharedAccessKey=LbQu05VqpRZeUAbMATl7OCOy8FVFp51FV1/6yFE168w=";
        public const string IoTHubUri  = "MoMoIoT.azure-devices.net";
        public const string deviceId  = "FESBTemperature";
        public const string DeviceKey  = "LbQu05VqpRZeUAbMATl7OCOy8FVFp51FV1/6yFE168w=";
    }
}
