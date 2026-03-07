using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.KeyBoard.Scripts;
using DiskayBot.Bot.Bot.Messages;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Callbacks.Account;

[CallbackName("changeProfileData", AccessLevel.User)]
public class ChangeProfileDataCallback : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        await ctx.Bot.EditMessageText(
            ctx.Event.Chat,
            ctx.Event.MessageId,
            MessageBuilder.ShowProfile(ctx.User!),
            ParseMode.Markdown,
            replyMarkup: GlobalKeyboard.GetProfileDataKeyboard(),
            cancellationToken: token
        );
    }
}
