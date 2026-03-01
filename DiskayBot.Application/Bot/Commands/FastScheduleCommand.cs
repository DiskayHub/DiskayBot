using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using DiskayBot.Services.ScheduleService.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/disky", AccessLevel.User)]
public class FastScheduleCommand : IBaseCommand {
    private readonly IScheduleController _schedule;

    public FastScheduleCommand(IScheduleController schedule) {
        _schedule = schedule;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var daySchedule = _schedule.GetActualSchedule($"ИТ{ctx.User!.group_name}");
        if (daySchedule != null) {
            var result = MessageBuilder.ShowSchedule(daySchedule);
            var keyboard = new InlineKeyboardMarkup(new[] {
                InlineKeyboardButton.WithCallbackData("Обновить 💫", "updateSchedule")
            });
            await ctx.Bot.SendMessage(ctx.Event.Chat, result, ParseMode.Html, replyMarkup: keyboard, cancellationToken: token);
        }
        else {
            await ctx.Bot.SendMessage(ctx.Event.Chat, "Не получилось отправить ближайшее расписание", ParseMode.Markdown, cancellationToken: token);
        }
    }
}
