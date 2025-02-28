using System.Collections.Generic;

namespace InBoostWeatherBot.Models
{
    public class UserWithHistory : User
    {
        public List<WeatherHistory> WeatherHistory { get; set; } = new();
    }
}