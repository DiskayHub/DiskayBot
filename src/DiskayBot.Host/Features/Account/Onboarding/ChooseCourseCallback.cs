using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Host.Features.Account.Onboarding;

[CallbackName("chooseCourse")]
public class ChooseCourseCallback : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var keyboard = GetKeyboard("chooseGroup");
        await ctx.Bot.EditMessageText(
            ctx.Event.Chat,
            ctx.Event.MessageId,
            "Выберите курс",
            ParseMode.Markdown,
            replyMarkup: keyboard,
            cancellationToken: token
        );
    }

    private static InlineKeyboardMarkup GetKeyboard(string nextCallback) {
        var courses = new[] { "1", "2", "3", "4" };
        var rows = courses.Select(c =>
            new[] { InlineKeyboardButton.WithCallbackData($"{c} курс", $"{nextCallback}={c}") }
        ).ToList();
        return new InlineKeyboardMarkup(rows);
    }
}
