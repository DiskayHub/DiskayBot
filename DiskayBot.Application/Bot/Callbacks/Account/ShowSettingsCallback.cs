using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.KeyBoard.Scripts;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Callbacks.Account;

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
