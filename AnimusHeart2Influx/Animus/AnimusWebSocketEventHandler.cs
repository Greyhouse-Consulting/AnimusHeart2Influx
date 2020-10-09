using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnimusHeart2Influx.Domain;
using AnimusHeart2Influx.Influx;
using Newtonsoft.Json;
using Serilog;

namespace AnimusHeart2Influx.Animus
{
    public interface IAnimusWebSocketEventHandler
    {
        Task Handle(string message);
        Task RefreshDevices();
    }

    public class AnimusWebSocketEventHandler : IAnimusWebSocketEventHandler
    {
        private readonly IInfluxService _influxService;
        private readonly AnimusHeartHttpClient _httpClient;

        IList<DeviceMeasure> _deviceMeasures;

        private readonly ILogger _logger = Log.ForContext<AnimusWebSocketEventHandler>();

        public AnimusWebSocketEventHandler(IInfluxService influxService, AnimusHeartHttpClient httpClient)
        {
            _influxService = influxService;
            _httpClient = httpClient;
        }

        public async Task Handle(string message)
        {
            if (_deviceMeasures == null)
                await RefreshDevices();

            if (message != "authenticated")
            {
                var o = JsonConvert.DeserializeObject<AnimusEvent>(message);

                var d = _deviceMeasures.FirstOrDefault(dm => dm.Id == o.functionUID);
                if (d != null)
                {
                    _logger.Debug("{name} - {level} {unit} ", d.Name, o.value.level, o.value.unit);
                    _influxService.Save(d.Id, d.Name, o.value.level, o.value.unit);
                }
                else
                {
                    _logger.Information("New device detected {device}. ", o.functionUID);

                    await RefreshDevices();

                    var d2 = _deviceMeasures.FirstOrDefault(dm => dm.Id.StartsWith(o.functionUID));
                    if (d2 == null)
                    {
                        _logger.Error("No metadata for device {device} after reloading metadata", o.functionUID);
                    }
                    else
                    {
                        _influxService.Save(d2.Id, d2.Name, o.value.level, o.value.unit);
                    }
                }
            }
            else
                _logger.Information("Laputa says: " + message);
        }

        public async Task RefreshDevices()
        {
            _logger.Information("Reloading device metadata");

            _deviceMeasures = new List<DeviceMeasure>();

            var devices = await _httpClient.GetDevices();

            foreach (var device in devices)
            {
                foreach (var primaryFunc in device.Properties.primary_funcs)
                {
                    _logger.Information("{name} {description} with funcs {function}", device.Properties.name,
                        device.Properties.description, primaryFunc);

                    _logger.Debug("{name} features {types} ", device.Properties.name, string.Join(",", device.Properties.types));
                    _deviceMeasures.Add(new DeviceMeasure
                    {
                        DeviceId = device.Properties.UID,
                        Id = primaryFunc,
                        Description = device.Properties.description,
                        Name = device.Properties.name,
                        Area = device.Properties.animus_area
                    });
                }
            }

            _logger.Information("Reloading devices complete");
        }
    }
}