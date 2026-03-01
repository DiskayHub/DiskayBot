using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Callbacks.Schedule;

[CallbackName("checkGroup", AccessLevel.User)]
public class CheckGroupCallback : IBaseCommand {
    private readonly MemoryController _memoryController;

    public CheckGroupCallback(MemoryController memoryController) {
        _memoryController = memoryController;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var callbackEvent = (CallbackQueryUserEvent)ctx.Event;
        if (callbackEvent.Query != null) {
            var course = short.Parse(callbackEvent.Query);
            var allGroups = await _memoryController.GetCourseGroups(course);
            if (allGroups != null) {
                allGroups = allGroups.OrderBy(c => {
                    var parts = c.name.Split('-');
                    return int.Parse(parts[1]);
                }).ToList();

                var buttons = allGroups.Select(group =>
                    InlineKeyboardButton.WithCallbackData(group.name, $"checkingSchedule={group.name}={course}")
                ).ToArray();

                var keyboard = new InlineKeyboardMarkup(new[] {
                    buttons,
                    new[] { InlineKeyboardButton.WithCallbackData("<-- Вернуться назад", "checkCoursesBack") }
                });

                await ctx.Bot.EditMessageText(
                    ctx.Event.Chat,
                    ctx.Event.MessageId,
                    $"*Курс: {course}*\n\nВыберите группу:",
                    ParseMode.Markdown,
                    replyMarkup: keyboard,
                    cancellationToken: token
                );
            }
        }
    }
}
