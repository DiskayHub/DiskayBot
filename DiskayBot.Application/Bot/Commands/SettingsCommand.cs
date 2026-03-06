using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/settings", AccessLevel.User)]
public class SettingsCommand : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var keyboard = new InlineKeyboardMarkup(new[] {
            InlineKeyboardButton.WithCallbackData("Изменить данные о профиле", "changeProfileData")
        });
        await ctx.Bot.SendMessage(ctx.Event.Chat, "Настройки", replyMarkup: keyboard, cancellationToken: token);
    }
}
