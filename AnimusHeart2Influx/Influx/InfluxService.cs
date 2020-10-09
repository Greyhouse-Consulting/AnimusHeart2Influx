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
                point = PointData.Measurement("humidity")
                    .Tag("id", id)
                    .Tag("name", name)
                    .Field("Humidity", amount)
                    .Timestamp(DateTime.UtcNow, WritePrecision.Ns);
            }
            else if (measure == "°C")
            {
                point = PointData.Measurement("temperature")
                    .Tag("id", id)
                    .Tag("name", name)
                    .Field("Temperature", amount)
                    .Timestamp(DateTime.UtcNow, WritePrecision.Ns);
            }

            if(point != null)
            {
                writeApi.WritePoint("Animus", "Animus", point);
            }
        }
    }
}