using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.KeyBoard.Scripts;
using DiskayBot.Bot.Bot.Messages;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
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
        var groupName = ctx.User!.group_name;
        var lookup = await _schedule.GetActualSchedule(groupName);
        var today = DateOnly.FromDateTime(DateTime.Now);

        switch (lookup.Status) {
            case ActualScheduleStatus.Found:
                await ctx.Bot.SendMessage(ctx.Event.Chat, MessageBuilder.ShowSchedule(lookup.Schedule!), ParseMode.Html,
                    replyMarkup: GlobalKeyboard.GetScheduleNavigatorKeyboard(lookup.Schedule!.date), cancellationToken: token);
                break;
            case ActualScheduleStatus.NoClasses:
                await ctx.Bot.SendMessage(ctx.Event.Chat, MessageBuilder.NoClassesThisWeek(today, groupName), ParseMode.Html,
                    replyMarkup: GlobalKeyboard.GetScheduleNavigatorKeyboard(today), cancellationToken: token);
                break;
            case ActualScheduleStatus.NotLoaded:
                await ctx.Bot.SendMessage(ctx.Event.Chat, MessageBuilder.ScheduleNotLoaded(), cancellationToken: token);
                break;
        }
    }
}
