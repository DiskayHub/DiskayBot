using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Presentation.Keyboards.Scripts;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;
using Telegram.Bot;

namespace DiskayBot.Host.Features.Account.Callbacks;

[CallbackName("showSettings", AccessLevel.User)]
public class ShowSettingsCallback : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        await ctx.Bot.EditMessageText(
            ctx.Event.Chat,
            ctx.Event.MessageId,
            "Настройки",
            replyMarkup: GlobalKeyboard.GetSettingsKeyboard(ctx.User!.notify),
            cancellationToken: token
        );
    }
}
