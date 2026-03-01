using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Callbacks.Account;

[CallbackName("changeProfileData", AccessLevel.User)]
public class ChangeProfileDataCallback : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var keyboard = new InlineKeyboardMarkup(new[] {
            InlineKeyboardButton.WithCallbackData("Изменить группу", "changeCourse")
        });
        await ctx.Bot.EditMessageText(
            ctx.Event.Chat,
            ctx.Event.MessageId,
            MessageBuilder.ShowProfile(ctx.User!),
            ParseMode.Markdown,
            replyMarkup: keyboard,
            cancellationToken: token
        );
    }
}
