using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using InBoostWeatherBot.Data;
using Telegram.Bot;


namespace InBoostWeatherBot.Services
{
    public class SwaggerApi
    {
        public static async Task StartApi()
        {
            var builder = WebApplication.CreateBuilder();

            string connectionString = "Server=localhost;Database=WeatherBotDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";

            builder.Services.AddControllers();
            builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(connectionString));
            builder.Services.AddScoped(_ => new WeatherBotDb(connectionString));
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<WeatherService>();
            builder.Services.AddScoped<WeatherHistoryService>();

            string botToken = "7980038447:AAHETXa0QyDLdW_wf4az0eaQ1Zb4-oK42pw";
            builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WeatherBot API", Version = "v1" });
            });

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherBot API V1");
                c.RoutePrefix = string.Empty;
            });

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
