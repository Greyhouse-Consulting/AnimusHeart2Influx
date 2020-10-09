using System;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace AnimusHeart2Influx.Influx
{
    public interface IInfluxService
    {
        void Save(string id, string name, double amount, string measure);
    }

    public class InfluxServiceService : IInfluxService
    {
        private readonly InfluxDBClient _influxDbClient;

        public InfluxServiceService(InfluxDBClient influxDbClient)
        {
            _influxDbClient = influxDbClient;
        }

        public void Save(string id, string name, double amount, string measure)
        {
            using var writeApi = _influxDbClient.GetWriteApi();
            var health = _influxDbClient.HealthAsync().GetAwaiter().GetResult();
            
            PointData point = null;
            
            if (measure == "%")
            {
                point = PointData.Measurement("sensor")
                    .Tag("id", id)
                    .Tag("name", name)
                    .Field("Humidity", amount)
                    .Timestamp(DateTime.UtcNow, WritePrecision.S);
            }
            else if (measure == "°C")
            {
                point = PointData.Measurement("sensor")
                    .Tag("id", id)
                    .Tag("name", name)
                    .Field("Temperature", amount)
                    .Timestamp(DateTime.UtcNow, WritePrecision.S);
            }

            if(point != null)
            {
                writeApi.WritePoint("AnimusHeart", "AnimusHeart", point);
            }
        }
    }
}