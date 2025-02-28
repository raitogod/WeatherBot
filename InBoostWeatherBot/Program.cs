using InBoostWeatherBot.Data;
using InBoostWeatherBot.Services;
using InBoostWeatherBot;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Task SwaggerApiTask = Task.Run(() => SwaggerApi.StartApi());

        var dbService = new WeatherBotDb("Server=localhost;Database=WeatherBotDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");
        var botService = new BotService("7980038447:AAHETXa0QyDLdW_wf4az0eaQ1Zb4-oK42pw",
            new UserService(dbService),
            new WeatherService(),
            new WeatherHistoryService(dbService));

        botService.Start();

        await Task.Delay(-1);
    }
}
