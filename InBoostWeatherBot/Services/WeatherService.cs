using InBoostWeatherBot.Models;
using System.Text.Json;

namespace InBoostWeatherBot.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "d567f4443092f711fe1102d0c89e4694";

        public WeatherService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<WeatherHistory?> GetWeatherAsync(string city)
        {
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={ApiKey}&units=metric&lang=ru";
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                return new WeatherHistory
                {
                    City = root.GetProperty("name").GetString(),
                    Temperature = root.GetProperty("main").GetProperty("temp").GetDouble(),
                    Description = root.GetProperty("weather")[0].GetProperty("description").GetString(),
                    Timestamp = DateTime.Now
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
