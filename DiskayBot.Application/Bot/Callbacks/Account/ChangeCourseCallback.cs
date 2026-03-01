using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Callbacks.Account;

[CallbackName("changeCourse", AccessLevel.User)]
public class ChangeCourseCallback : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var courses = new[] { "1", "2", "3", "4" };
        var rows = courses.Select(c =>
            new[] { InlineKeyboardButton.WithCallbackData($"{c} курс", $"changeGroup={c}") }
        ).ToList();
        var keyboard = new InlineKeyboardMarkup(rows);

        await ctx.Bot.EditMessageText(
            ctx.Event.Chat,
            ctx.Event.MessageId,
            "Выберите курс",
            ParseMode.Markdown,
            replyMarkup: keyboard,
            cancellationToken: token
        );
    }
}
