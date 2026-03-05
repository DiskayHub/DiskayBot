using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.Messages;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Redis;
using DiskayBot.Redis.Abstractions;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Callbacks.Data;

[CallbackName("preCreateAccountOffer")]
public class PreCreateAccountOfferCallback : IBaseCommand {
    private readonly IRedisController _redis;

    public PreCreateAccountOfferCallback(IRedisController redis) {
        _redis = redis;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var callbackEvent = (CallbackQueryUserEvent)ctx.Event;

        if (callbackEvent.Query != null) {
            var hash = new HashEntry[] {
                new("group_id", callbackEvent.Query)
            };
            await _redis.SaveDataHash(ctx.Event.Chat.Id.ToString(), hash, TimeSpan.FromSeconds(60));
        }

        var keyboard = new InlineKeyboardMarkup(new[] {
            InlineKeyboardButton.WithCallbackData("Да", "createAccount=yes"),
            InlineKeyboardButton.WithCallbackData("Нет", "createAccount=no")
        });

        await ctx.Bot.EditMessageText(
            ctx.Event.Chat,
            ctx.Event.MessageId,
            MessageBuilder.RegisterOffer(),
            ParseMode.Markdown,
            replyMarkup: keyboard,
            cancellationToken: token
        );
    }
}
