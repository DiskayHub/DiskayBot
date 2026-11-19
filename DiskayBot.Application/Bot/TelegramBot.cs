using DiskayBot.Bot.Bot.Options;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Events.Base;
using DiskayBot.Bot.Middleware;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace DiskayBot.Bot.Bot;

public class TelegramBot : BackgroundService {
    private TelegramBotClient _bot;
    private ILogger<TelegramBot> _logger;
    private ILoggerFactory _loggerFactory;
    private readonly BotMiddleware _middleware;
    private static readonly TimeSpan PollingRetryDelay = TimeSpan.FromSeconds(5);
    
    public TelegramBot(IOptions<TelegramBotOptions> options, BotMiddleware middleware, ILogger<TelegramBot> logger, ILoggerFactory loggerFactory) {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _middleware = middleware;
        _bot = new TelegramBotClient(options.Value.Token);
        _bot.OnUpdate += OnUpdate;
        _bot.OnError += OnError;
    }

    protected async Task OnUpdate(Update update) {
        var userEvent = EventCreator.Create(update);
        if (userEvent == null) {
            _logger.LogDebug("Пропущен update {UpdateId} неподдерживаемого типа {UpdateType}", update.Id, update.Type);
            return;
        }

        CancellationTokenSource ctsToken = new(TimeSpan.FromSeconds(5));
        
        var cts = new BotContext {
            Bot = _bot,
            Event = userEvent
        };
        await _middleware.InvokeAsync(cts, ctsToken.Token);
    }

    private async Task OnError(Exception exception, HandleErrorSource source) {
        if (source != HandleErrorSource.PollingError) {
            _logger.LogError(exception, "Ошибка Telegram ({Source})", source);
            return;
        }

        // Polling повторяется сразу после возврата из обработчика: без паузы сетевой сбой даёт десятки записей в секунду
        _logger.LogWarning(exception, "Ошибка получения обновлений Telegram, повтор через {RetryDelay} с", PollingRetryDelay.TotalSeconds);
        await Task.Delay(PollingRetryDelay);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try{
            _logger.LogInformation("Запуск Telegram бота..");
            var botInfo = await _bot.GetMe();
            _logger.LogInformation("@{BotUsername} запущен", botInfo.Username);
                
            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception ex){
            _logger.LogCritical(ex, "Критическая ошибка при запуске бота");
        }
    }
}
