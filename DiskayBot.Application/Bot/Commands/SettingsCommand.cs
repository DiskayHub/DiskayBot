using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.KeyBoard.Scripts;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Commands;

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
