using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.DTOs;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace DiskayBot.Bot.Middleware;

public class BotMiddleware {
    private readonly ILogger<BotMiddleware> _logger;
    private readonly CommandDispatcher _commandDispatcher;
    
    public BotMiddleware(ILogger<BotMiddleware> logger, CommandDispatcher commandDispatcher) {
        _logger = logger;
        _commandDispatcher = commandDispatcher;
    }

    public async Task InvokeAsync(BotContext botContext, CancellationToken cancellationToken) {
        _logger.LogDebug($"Обработка запроса от пользователя {botContext.Event.Username}: {botContext.Event.GetContent()}");
        try {
            await _commandDispatcher.DispatchAsync(botContext, cancellationToken);
        }
        catch (Exception ex) {
            
        }
    }
}