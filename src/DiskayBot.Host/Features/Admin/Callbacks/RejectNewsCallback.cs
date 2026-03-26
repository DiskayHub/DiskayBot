using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;
using DiskayBot.Infrastructure.Redis.Abstractions;
using Telegram.Bot;

namespace DiskayBot.Host.Features.Admin.Callbacks;

[CallbackName("rejectNews", AccessLevel.Admin)]
public class RejectNewsCallback : IBaseCommand {
    private readonly IRedisController _redis;

    public RejectNewsCallback(IRedisController redis) {
        _redis = redis;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var redisKey = $"admin:{ctx.Event.UserId}:news";
        await _redis.DeleteData(redisKey);

        await ctx.Bot.EditMessageText(
            ctx.Event.Chat,
            ctx.Event.MessageId,
            "❌ Рассылка отклонена.",
            cancellationToken: token
        );
    }
}
