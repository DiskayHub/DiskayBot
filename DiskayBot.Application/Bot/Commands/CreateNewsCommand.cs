using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.Messages;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Redis.Abstractions;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/create_news", AccessLevel.Admin)]
public class CreateNewsCommand : IBaseCommand {
    private readonly IRedisController _redis;

    public CreateNewsCommand(IRedisController redis) {
        _redis = redis;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var parts = ctx.Event.GetContent().Split(' ', 2);
        if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1])) {
            await ctx.Bot.SendMessage(ctx.Event.Chat, "Укажите текст рассылки:\n/create_news <текст>", cancellationToken: token);
            return;
        }

        var newsText = parts[1].Trim();
        var redisKey = $"admin:{ctx.Event.UserId}:news";

        await _redis.SaveDataHash(redisKey, [new HashEntry("text", newsText)], TimeSpan.FromMinutes(15));

        var keyboard = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData("✅ Разослать", "confirmNews"),
                InlineKeyboardButton.WithCallbackData("❌ Отклонить", "rejectNews"),
            ]
        ]);

        await ctx.Bot.SendMessage(
            ctx.Event.Chat,
            MessageBuilder.NewsPreview(newsText),
            ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: token
        );
    }
}
