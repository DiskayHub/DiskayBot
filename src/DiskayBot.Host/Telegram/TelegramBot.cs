using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Telegram.Events.Base;
using DiskayBot.Host.Telegram.Middleware;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DiskayBot.Host.Telegram;

public class TelegramBot : BackgroundService {
    private TelegramBotClient _bot;
    private ILogger<TelegramBot> _logger;
    private ILoggerFactory _loggerFactory;
    private readonly BotMiddleware _middleware;
    
    public TelegramBot(IOptions<TelegramBotOptions> options, BotMiddleware middleware, ILogger<TelegramBot> logger, ILoggerFactory loggerFactory) {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _middleware = middleware;
        _bot = new TelegramBotClient(options.Value.Token);
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
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
