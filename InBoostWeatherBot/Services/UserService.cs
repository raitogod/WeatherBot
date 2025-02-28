using Dapper;
using InBoostWeatherBot.Data;
using InBoostWeatherBot.Models;

namespace InBoostWeatherBot.Services
{
    public class UserService
    {
        private readonly WeatherBotDb _weatherbotdb;

        public UserService(WeatherBotDb databaseService)
        {
            _weatherbotdb = databaseService;
        }

        public async Task AddUserAsync(User user)
        {
            using var connection = _weatherbotdb.GetConnection();

            Console.WriteLine($"User: {user.TelegramId}, Username: {user.Username ?? "NULL"}");

            string checkQuery = "SELECT COUNT(*) FROM Users WHERE TelegramId = @TelegramId";
            int count = await connection.ExecuteScalarAsync<int>(checkQuery, new { user.TelegramId });

            if (count == 0)
            {
                string username = string.IsNullOrEmpty(user.Username) ? $"AnonymousUser_{user.TelegramId}" : user.Username;

                string insertQuery = "INSERT INTO Users (TelegramId, Username) VALUES (@TelegramId, @Username)";
                await connection.ExecuteAsync(insertQuery, new { user.TelegramId, Username = username });

                Console.WriteLine($"Добавлен пользователь: {user.TelegramId}, Username: {username}");
            }
        }


        public async Task<User?> GetUserAsync(int id)
        {
            using var connection = _weatherbotdb.GetConnection();

            string userQuery = "SELECT * FROM Users WHERE Id = @Id";
            var user = await connection.QueryFirstOrDefaultAsync<User>(userQuery, new { Id = id });

            if (user == null)
                return null;

            string historyQuery = "SELECT * FROM WeatherHistory WHERE UserId = @UserId ORDER BY Timestamp DESC";
            var history = await connection.QueryAsync<WeatherHistory>(historyQuery, new { UserId = id });

            return new UserWithHistory
            {
                Id = user.Id,
                TelegramId = user.TelegramId,
                Username = user.Username,
                WeatherHistory = history.ToList()
            };
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            using var connection = _weatherbotdb.GetConnection();
            string query = "SELECT * FROM Users";
            var users = await connection.QueryAsync<User>(query);
            return users.ToList();
        }

    }
}
