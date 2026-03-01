using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Events.Base;
using DiskayBot.Bot.Middleware;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DiskayBot.Bot.Bot;

public class TelegramBot {
    private TelegramBotClient _bot;
    private ILogger<TelegramBot> _logger;
    private ILoggerFactory _loggerFactory;
    private readonly BotMiddleware _middleware;
    
    public TelegramBot(string botToken, BotMiddleware middleware, ILogger<TelegramBot> logger, ILoggerFactory loggerFactory) {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _middleware = middleware;
        _bot = new TelegramBotClient(botToken);
        _bot.OnUpdate += OnUpdate;
    }

    protected async Task OnUpdate(Update update) {
        CancellationTokenSource ctsToken = new(TimeSpan.FromSeconds(5));
        
        var cts = new BotContext {
            Bot = _bot,
            Event = EventCreator.Create(update)
        };
        await _middleware.InvokeAsync(cts, ctsToken.Token);
    }

    public async Task Start() {
        try{
            _logger.LogInformation("Запуск Telegram бота..");
            var botInfo = await _bot.GetMe();
            _logger.LogInformation($"{botInfo.Username} запущен!");
                
            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception ex){
            _logger.LogCritical(ex, "Критическая ошибка при запуске бота");
        }
    }
}
