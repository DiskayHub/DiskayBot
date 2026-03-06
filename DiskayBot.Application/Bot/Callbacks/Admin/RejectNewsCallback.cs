using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Redis.Abstractions;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Callbacks.Admin;

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
