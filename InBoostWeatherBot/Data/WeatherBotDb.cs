using System.Data;
using Microsoft.Data.SqlClient;

namespace InBoostWeatherBot.Data
{
    public class WeatherBotDb
    {
        private readonly string _connectionString;

        public WeatherBotDb(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
