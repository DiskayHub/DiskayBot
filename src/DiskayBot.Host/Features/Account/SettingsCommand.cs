using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Presentation.Keyboards.Scripts;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;
using Telegram.Bot;

namespace DiskayBot.Host.Features.Account;

[CommandName("/settings", AccessLevel.User)]
public class SettingsCommand : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        await ctx.Bot.SendMessage(
            ctx.Event.Chat,
            "Настройки",
            replyMarkup: GlobalKeyboard.GetSettingsKeyboard(ctx.User!.notify),
            cancellationToken: token
        );
    }
}
