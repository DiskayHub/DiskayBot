using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Features.Account;
using DiskayBot.Host.Presentation.Messages;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;
using DiskayBot.Infrastructure.Redis.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Host.Features.Admin.Callbacks;

[CallbackName("confirmNews", AccessLevel.Admin)]
public class ConfirmNewsCallback : IBaseCommand {
    private readonly IRedisController _redis;
    private readonly MemoryController _memoryController;

    public ConfirmNewsCallback(IRedisController redis, MemoryController memoryController) {
        _redis = redis;
        _memoryController = memoryController;
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
        if (users != null) {
            foreach (var user in users) {
                try {
                    await ctx.Bot.SendMessage(user.user_id, MessageBuilder.NewsText(newsText), ParseMode.Html, cancellationToken: token);
                    sent++;
                }
                catch {
                    // пользователь мог заблокировать бота
                }
            }
        }

        await _redis.DeleteData(redisKey);

        await ctx.Bot.EditMessageText(
            ctx.Event.Chat,
            ctx.Event.MessageId,
            $"✅ Рассылка отправлена {sent} пользователям.",
            cancellationToken: token
        );
    }
}
