using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace InBoostWeatherBot.Services
{
    public class BotService
    {
        private readonly ITelegramBotClient _client;
        private readonly UserService _userService;
        private readonly WeatherService _weatherService;
        private readonly WeatherHistoryService _weatherHistoryService;
        private bool waitingForCity = false;

        public BotService(string token, UserService userService, WeatherService weatherService, WeatherHistoryService weatherHistoryService)
        {
            _client = new TelegramBotClient(token);
            _userService = userService;
            _weatherService = weatherService;
            _weatherHistoryService = weatherHistoryService;
        }

        public void Start()
        {
            _client.StartReceiving(Updater, Errors);
            Console.WriteLine("Бот запущен...");
        }

        private async Task Updater(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                var callback = update.CallbackQuery;
                int chatId = (int)callback.Message.Chat.Id;

                Console.WriteLine($"Обработан CallbackQuery: {callback.Data}");

                if (callback.Data == "input_city" || callback.Data == "get_weather_again")
                {
                    waitingForCity = true;
                    await client.SendMessage(
                        chatId: chatId,
                        text: "Введите название вашего города:",
                        cancellationToken: token
                    );
                }
                else if (callback.Data == "exit")
                {
                    await client.SendMessage(
                        chatId: chatId,
                        text: "Спасибо за использование бота! До свидания!",
                        cancellationToken: token
                    );
                }

                await client.AnswerCallbackQuery(callback.Id, cancellationToken: token);
                return;
            }

            if (update.Type == UpdateType.Message && update.Message.Text != null)
            {
                int chatId = (int)update.Message.Chat.Id;
                string? username = update.Message.From?.Username;
                string messageText = update.Message.Text.Trim();

                await _userService.AddUserAsync(new Models.User { TelegramId = chatId, Username = username });

                if (messageText.StartsWith("/weather", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = messageText.Split(' ', 2);
                    if (parts.Length < 2)
                    {
                        await client.SendMessage(chatId, "Введите команду в формате: /weather {город}", cancellationToken: token);
                        return;
                    }

                    string city = parts[1].Trim();
                    var weatherData = await _weatherService.GetWeatherAsync(city);

                    if (weatherData != null)
                    {
                        string weatherInfo = $"Погода в городе {weatherData.City}: {weatherData.Temperature}°C, {weatherData.Description}.";
                        await client.SendMessage(chatId, weatherInfo, cancellationToken: token);

                        await _weatherHistoryService.SaveWeatherAsync(chatId, weatherData.City, weatherData.Temperature, weatherData.Description);
                    }
                    else
                    {
                        await client.SendMessage(chatId, "Не удалось получить погоду. Проверьте название города.", cancellationToken: token);
                    }

                    return;
                }

                if (waitingForCity)
                {
                    string city = messageText;
                    var weatherData = await _weatherService.GetWeatherAsync(city);

                    if (weatherData != null)
                    {
                        string weatherInfo = $"Погода в городе {weatherData.City}: {weatherData.Temperature}°C, {weatherData.Description}.";
                        await client.SendMessage(chatId, weatherInfo, cancellationToken: token);

                        await _weatherHistoryService.SaveWeatherAsync(chatId, weatherData.City, weatherData.Temperature, weatherData.Description);

                        InlineKeyboardMarkup optionsKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Узнать погоду еще", "get_weather_again"),
                                InlineKeyboardButton.WithCallbackData("Выйти", "exit")
                            }
                        });

                        await client.SendMessage(
                            chatId: chatId,
                            text: "Что хотите сделать дальше?",
                            replyMarkup: optionsKeyboard,
                            cancellationToken: token
                        );
                    }
                    else
                    {
                        await client.SendMessage(chatId, "Не удалось получить погоду. Проверьте название города.", cancellationToken: token);
                    }

                    waitingForCity = false;
                    return;
                }

                if (messageText.Equals("/start", StringComparison.OrdinalIgnoreCase))
                {
                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Ввести город", "input_city")
                    });

                    await client.SendMessage(
                        chatId: chatId,
                        text: "Добро пожаловать! Напишите /weather и название города либо нажмите кнопку ниже и введите ваш город для получения погоды:",
                        replyMarkup: inlineKeyboard,
                        cancellationToken: token
                    );
                }
            }
        }

        private Task Errors(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
