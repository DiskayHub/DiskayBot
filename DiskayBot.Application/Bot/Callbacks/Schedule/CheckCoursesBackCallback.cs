using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.KeyBoard.Scripts;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Callbacks.Schedule;

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
