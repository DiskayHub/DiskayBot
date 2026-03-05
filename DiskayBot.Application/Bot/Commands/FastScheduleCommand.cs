using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.KeyBoard.Scripts;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using DiskayBot.Bot.ScheduleService;
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
        var daySchedule = await _schedule.GetActualSchedule($"ИТ{ctx.User!.group_name}");
        if (daySchedule != null) {
            var result = MessageBuilder.ShowSchedule(daySchedule);
            await ctx.Bot.SendMessage(ctx.Event.Chat, result, ParseMode.Html, replyMarkup: GlobalKeyboard.GetScheduleNavigatorKeyboard(daySchedule.date), cancellationToken: token);
        }
        else {
            await ctx.Bot.SendMessage(ctx.Event.Chat, "Не получилось отправить ближайшее расписание", ParseMode.Markdown, cancellationToken: token);
        }
    }
}
