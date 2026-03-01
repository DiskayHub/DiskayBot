using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Redis;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Callbacks.Account;

[CallbackName("changeGroup", AccessLevel.User)]
public class ChangeGroupCallback : IBaseCommand {
    private readonly MemoryController _memoryController;
    private readonly RedisController _redis;

    public ChangeGroupCallback(MemoryController memoryController, RedisController redis) {
        _memoryController = memoryController;
        _redis = redis;
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
                    InlineKeyboardButton.WithCallbackData(group.name, $"changingGroup={group.id}")
                ).ToArray();

                var keyboard = new InlineKeyboardMarkup(new[] {
                    buttons,
                    new[] { InlineKeyboardButton.WithCallbackData("<-- Вернуться назад", "changeCourse") }
                });

                // Save group_id to Redis for the changingGroup callback
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
