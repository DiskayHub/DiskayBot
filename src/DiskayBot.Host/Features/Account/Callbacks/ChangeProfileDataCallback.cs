using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Presentation.Keyboards.Scripts;
using DiskayBot.Host.Presentation.Messages;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Host.Features.Account.Callbacks;

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
