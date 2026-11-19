using System.Diagnostics;
using DiskayBot.API.Exeptions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Bot.Messages;
using DiskayBot.Bot.DTOs;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;


namespace DiskayBot.Bot.Middleware;

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
        var userEvent = botContext.Event;
        var startedAt = Stopwatch.GetTimestamp();
        try {
            botContext.User = await _memoryController.GetUser(userEvent.UserId);
            await _commandDispatcher.DispatchAsync(botContext, cancellationToken);
            LogHandled(botContext, startedAt);
        }
        catch (NotAdminException) {
            _logger.LogWarning("Пользователь {UserId} (@{Username}) пытался вызвать админ-команду '{Content}'",
                userEvent.UserId, userEvent.Username, userEvent.GetContent());
            await botContext.Bot.SendMessage(
                    botContext.Event.Chat, 
                $"Ты думал я настолько глуп? ☠️ \n<i>*Презрительный взгляд на <b>{botContext.Event.Username}</b>..</i>",
                ParseMode.Html,
                cancellationToken: cancellationToken
            );
        }
        catch (NotAuthorizatedExeption) {
            _logger.LogDebug("Незарегистрированный пользователь {UserId} вызвал '{Content}'",
                userEvent.UserId, userEvent.GetContent());
            await botContext.Bot.SendMessage(
                botContext.Event.Chat,
                MessageBuilder.NotRegistered(),
                ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        catch (ConnectionRefuseExeption ex) {
            _logger.LogError(ex, "Сервис {Service} недоступен при обработке '{Content}' от пользователя {UserId}",
                ex.ServiceName, userEvent.GetContent(), userEvent.UserId);
            await botContext.Bot.SendMessage(
                botContext.Event.Chat,
                $"Сервис *{ex.ServiceName}* временно недоступен. Попробуйте позже.",
                ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        catch (HttpRequestException ex) {
            _logger.LogError(ex, "Ошибка HTTP запроса при обработке '{Content}' от пользователя {UserId}",
                userEvent.GetContent(), userEvent.UserId);
            await botContext.Bot.SendMessage(
                botContext.Event.Chat,
                "Произошла ошибка при обращении к сервису. Попробуйте позже.",
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Необработанная ошибка при обработке '{Content}' от пользователя {UserId}",
                userEvent.GetContent(), userEvent.UserId);
        }
    }

    private void LogHandled(BotContext botContext, long startedAt) {
        var userEvent = botContext.Event;
        if (botContext.Descriptor == null) {
            _logger.LogDebug("Сообщение от пользователя {UserId} не является командой: '{Content}'",
                userEvent.UserId, userEvent.GetContent());
            return;
        }

        _logger.LogInformation("{Kind} {Name} от пользователя {UserId} (@{Username}) обработан за {ElapsedMs:F0} мс",
            botContext.Descriptor.Kind, botContext.Descriptor.Name, userEvent.UserId, userEvent.Username,
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
    }
}
