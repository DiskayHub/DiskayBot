using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Presentation.Keyboards.Scripts;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Host.Features.Schedule;

[CommandName("/check", AccessLevel.User)]
public class CheckSchedulesCommand : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var messageText = "Проверить расписание 🔍\n\n*Diskay* покажет ближайшее расписание (поэтому смотри на дату)";
        var keyboard = GlobalKeyboard.GetCoursesKeyboard("checkGroup");
        await ctx.Bot.SendMessage(ctx.Event.Chat, messageText, ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: token);
    }
}
