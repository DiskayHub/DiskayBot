using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Presentation.Keyboards.Scripts;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Host.Features.Schedule.Callbacks;

[CallbackName("checkCoursesBack", AccessLevel.User)]
public class CheckCoursesBackCallback : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var messageText = "Проверить расписание 🔍\n\n*Diskay* покажет ближайшее расписание (поэтому смотри на дату)";
        var keyboard = GlobalKeyboard.GetCoursesKeyboard("checkGroup");
        await ctx.Bot.EditMessageText(
            ctx.Event.Chat,
            ctx.Event.MessageId,
            messageText,
            ParseMode.Markdown,
            replyMarkup: keyboard,
            cancellationToken: token
        );
    }
}
