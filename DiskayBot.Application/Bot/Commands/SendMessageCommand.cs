using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Messages;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/send_message", AccessLevel.Admin)]
public class SendMessageCommand : IBaseCommand {
    private readonly MemoryController _memoryController;
    private readonly ILogger<SendMessageCommand> _logger;

    public SendMessageCommand(MemoryController memoryController, ILogger<SendMessageCommand> logger) {
        _memoryController = memoryController;
        _logger = logger;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var parts = ctx.Event.GetContent().Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3 || string.IsNullOrWhiteSpace(parts[2])) {
            await ctx.Bot.SendMessage(ctx.Event.Chat, "Укажите тег и текст сообщения:\n/send_message @тег <текст>", cancellationToken: token);
            return;
        }

        var tag = parts[1].TrimStart('@');
        var users = await _memoryController.GetAllUsers();
        var user = users?.FirstOrDefault(u => string.Equals(u.username.TrimStart('@'), tag, StringComparison.OrdinalIgnoreCase));

        if (user == null) {
            await ctx.Bot.SendMessage(ctx.Event.Chat, $"Пользователь @{tag} не найден среди зарегистрированных.", cancellationToken: token);
            return;
        }

        try {
            await ctx.Bot.SendMessage(user.user_id, MessageBuilder.NewsText(parts[2].Trim()), ParseMode.Html, cancellationToken: token);
        }
        catch (ApiRequestException ex) {
            _logger.LogWarning(ex, "Не удалось отправить сообщение пользователю {UserId}", user.user_id);
            await ctx.Bot.SendMessage(ctx.Event.Chat, $"Не удалось отправить сообщение @{tag}: {ex.Message}", cancellationToken: token);
            return;
        }

        _logger.LogInformation("Админ {AdminId} отправил личное сообщение пользователю {UserId}", ctx.Event.UserId, user.user_id);
        await ctx.Bot.SendMessage(ctx.Event.Chat, $"✅ Сообщение отправлено @{tag}.", cancellationToken: token);
    }
}
