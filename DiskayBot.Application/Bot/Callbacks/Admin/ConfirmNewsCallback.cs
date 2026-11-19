using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Messages;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Redis.Abstractions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Callbacks.Admin;

[CallbackName("confirmNews", AccessLevel.Admin)]
public class ConfirmNewsCallback : IBaseCommand {
    private readonly IRedisController _redis;
    private readonly MemoryController _memoryController;
    private readonly ILogger<ConfirmNewsCallback> _logger;

    public ConfirmNewsCallback(IRedisController redis, MemoryController memoryController, ILogger<ConfirmNewsCallback> logger) {
        _redis = redis;
        _memoryController = memoryController;
        _logger = logger;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var redisKey = $"admin:{ctx.Event.UserId}:news";
        var hash = await _redis.GetDataHash(redisKey);

        if (hash == null) {
            await ctx.Bot.EditMessageText(ctx.Event.Chat, ctx.Event.MessageId, "Рассылка не найдена или истекло время.", cancellationToken: token);
            return;
        }

        var newsText = hash.FirstOrDefault(h => h.Name == "text").Value.ToString();
        var users = await _memoryController.GetNotifyUsers();

        var sent = 0;
        var failed = 0;
        if (users != null) {
            foreach (var user in users) {
                try {
                    await ctx.Bot.SendMessage(user.user_id, MessageBuilder.NewsText(newsText), ParseMode.Html, cancellationToken: token);
                    sent++;
                }
                catch (ApiRequestException ex) when (ex.ErrorCode == 403) {
                    // Пользователь заблокировал бота — штатная ситуация
                    failed++;
                    _logger.LogDebug("Рассылка: пользователь {UserId} заблокировал бота", user.user_id);
                }
                catch (Exception ex) {
                    failed++;
                    _logger.LogWarning(ex, "Рассылка: не удалось отправить сообщение пользователю {UserId}", user.user_id);
                }
            }
        }

        _logger.LogInformation("Рассылка отправлена: доставлено {Sent}, не доставлено {Failed}", sent, failed);

        await _redis.DeleteData(redisKey);

        await ctx.Bot.EditMessageText(
            ctx.Event.Chat,
            ctx.Event.MessageId,
            $"✅ Рассылка отправлена {sent} пользователям.",
            cancellationToken: token
        );
    }
}
