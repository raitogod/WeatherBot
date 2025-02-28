using Microsoft.AspNetCore.Mvc;
using InBoostWeatherBot.Services;
using Telegram.Bot;

namespace InBoostWeatherBot.Controllers
{
    [ApiController]
    [Route("api/weather")]
    public class WeatherController : ControllerBase
    {
        private readonly WeatherService _weatherService;
        private readonly UserService _userService;
        private readonly WeatherHistoryService _weatherHistoryService;
        private readonly ITelegramBotClient _client;

        public WeatherController(WeatherService weatherService, UserService userService, WeatherHistoryService weatherHistoryService, ITelegramBotClient client)
        {
            _weatherService = weatherService;
            _userService = userService;
            _weatherHistoryService = weatherHistoryService;
            _client = client;
        }

        [HttpPost("sendWeatherToAll/{city}")]
        public async Task<IActionResult> SendWeatherToAll(string city)
        {
            var users = await _userService.GetAllUsersAsync();
            if (users == null || !users.Any())
            {
                return NotFound("Нет зарегистрированных пользователей.");
            }

            var weatherData = await _weatherService.GetWeatherAsync(city);
            if (weatherData == null)
            {
                return BadRequest($"Не удалось получить данные о погоде для {city}.");
            }

            string weatherInfo = $"Погода в городе {weatherData.City}: {weatherData.Temperature}°C, {weatherData.Description}.";

            foreach (var user in users)
            {
                await _client.SendMessage(user.TelegramId, weatherInfo);
                await _weatherHistoryService.SaveWeatherAsync(user.TelegramId, weatherData.City, weatherData.Temperature, weatherData.Description);
            }

            return Ok($"Погода для {city} отправлена всем пользователям.");
        }

        [HttpPost("sendWeatherToUser/{id}/{city}")]
        public async Task<IActionResult> SendWeatherToUser(int id, string city)
        {
            var user = await _userService.GetUserAsync(id);
            if (user == null)
            {
                return NotFound($"Пользователь с ID {id} не найден.");
            }

            var weatherData = await _weatherService.GetWeatherAsync(city);
            if (weatherData == null)
            {
                return BadRequest("Не удалось получить данные о погоде.");
            }

            string weatherInfo = $"Погода в городе {weatherData.City}: {weatherData.Temperature}°C, {weatherData.Description}.";
            await _client.SendMessage(user.TelegramId, weatherInfo);
            await _weatherHistoryService.SaveWeatherAsync(user.TelegramId, weatherData.City, weatherData.Temperature, weatherData.Description);

            return Ok($"Погода для {city} отправлена пользователю {user.TelegramId}.");
        }
    }
}
