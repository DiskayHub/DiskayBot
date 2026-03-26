using DiskayBot.Infrastructure.Exeptions;
using DiskayBot.Host.Features.Account;
using DiskayBot.Host.Telegram.Exceptions;
using DiskayBot.Host.Presentation.Messages;
using DiskayBot.Host.Telegram.DTOs;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;


namespace DiskayBot.Host.Telegram.Middleware;

public class BotMiddleware {
    private readonly ILogger<BotMiddleware> _logger;
    private readonly CommandDispatcher _commandDispatcher;
    private readonly MemoryController _memoryController;

    public BotMiddleware(ILogger<BotMiddleware> logger, CommandDispatcher commandDispatcher, MemoryController memoryController) {
        _logger = logger;
        _commandDispatcher = commandDispatcher;
        _memoryController = memoryController;
    }

    public async Task InvokeAsync(BotContext botContext, CancellationToken cancellationToken) {
        _logger.LogDebug($"Обработка запроса от пользователя {botContext.Event.Username}: {botContext.Event.GetContent()}");
        try {
            botContext.User = await _memoryController.GetUser(botContext.Event.UserId);
            await _commandDispatcher.DispatchAsync(botContext, cancellationToken);
        }
        catch (NotAdminException) {
            await botContext.Bot.SendMessage(
                    botContext.Event.Chat, 
                $"Ты думал я настолько глуп? ☠️ \n<i>*Презрительный взгляд на <b>{botContext.Event.Username}</b>..</i>",
                ParseMode.Html,
                cancellationToken: cancellationToken
            );
        }
        catch (NotAuthorizatedExсeption) {
            await botContext.Bot.SendMessage(
                botContext.Event.Chat,
                MessageBuilder.NotRegistered(),
                ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        catch (ConnectionRefuseExeption ex) {
            _logger.LogError(ex, "Сервис {Service} недоступен", ex.ServiceName);
            await botContext.Bot.SendMessage(
                botContext.Event.Chat,
                $"Сервис *{ex.ServiceName}* временно недоступен. Попробуйте позже.",
                ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        catch (HttpRequestException ex) {
            _logger.LogError(ex, "Ошибка HTTP запроса");
            await botContext.Bot.SendMessage(
                botContext.Event.Chat,
                "Произошла ошибка при обращении к сервису. Попробуйте позже.",
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Необработанная ошибка при обработке запроса");
        }
    }
}
