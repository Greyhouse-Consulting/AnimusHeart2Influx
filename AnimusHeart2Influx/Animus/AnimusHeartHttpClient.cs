using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AnimusHeart2Influx.Animus
{
    public class AnimusHeartHttpClient
    {
        private readonly HttpClient _httpClient;

        public AnimusHeartHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Device>> GetDevices()
        {
            var response = await _httpClient.GetAsync(AnimusEndpoints.Devices);

            if (response.IsSuccessStatusCode)
            {
                return Converter.ParseDevices(await response.Content.ReadAsStringAsync());
            }

            throw new Exception("Failed to get devices from animus");
        }
    }
}
