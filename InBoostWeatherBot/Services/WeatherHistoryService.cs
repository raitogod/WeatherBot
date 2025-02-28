using Dapper;
using InBoostWeatherBot.Data;

namespace InBoostWeatherBot.Services
{
    public class WeatherHistoryService
    {
        private readonly WeatherBotDb _databaseService;

        public WeatherHistoryService(WeatherBotDb databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task SaveWeatherAsync(long telegramId, string city, double temperature, string description)
        {
            using var connection = _databaseService.GetConnection();

            string getUserIdQuery = "SELECT Id FROM Users WHERE TelegramId = @TelegramId;";
            int? userId = await connection.QueryFirstOrDefaultAsync<int?>(getUserIdQuery, new { TelegramId = telegramId });

            if (userId == null)
            {
                Console.WriteLine($"Ошибка: пользователь с TelegramId {telegramId} не найден.");
                return;
            }

            string insertQuery = @"
                INSERT INTO WeatherHistory (UserId, City, Temperature, Description)
                VALUES (@UserId, @City, @Temperature, @Description);";

            await connection.ExecuteAsync(insertQuery, new { UserId = userId, City = city, Temperature = temperature, Description = description });
        }


    }
}
