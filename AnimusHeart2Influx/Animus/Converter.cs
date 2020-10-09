using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnimusHeart2Influx.Animus
{
    public class Device
    {
        public Properties13 Properties { get; set; }    
    }

    public class Converter
    {

        public static IEnumerable<Device> ParseDevices(string json)
        {
            dynamic dynObj = JsonConvert.DeserializeObject(json);

            var jObj = (JObject)dynObj;

            var tokens = jObj.Children().ToList();

            var devices = new List<Device>();

            foreach (var jToken in tokens)
            {
                if (jToken is JProperty prop)
                {
                    devices.Add(prop.Value.ToObject<Device>());
                }
            }

            return devices;
        }
    }
}
