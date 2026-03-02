using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Callbacks.Schedule;

// [CallbackName("checkingSchedule", AccessLevel.User)]
// public class CheckScheduleCallback : IBaseCommand {
//     private readonly IScheduleController _scheduleService;
//
//     public CheckScheduleCallback(IScheduleController scheduleService) {
//         _scheduleService = scheduleService;
//     }
//
//     public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
//         var callbackEvent = (CallbackQueryUserEvent)ctx.Event;
//         if (callbackEvent.Query != null) {
//             var schedule = _scheduleService.GetActualSchedule($"ИТ{callbackEvent.Query}");
//             if (schedule != null) {
//                 var course = callbackEvent.QueryArgs?[0] ?? callbackEvent.Query;
//                 var keyboard = new InlineKeyboardMarkup(new[] {
//                     new[] { InlineKeyboardButton.WithCallbackData("<-- Вернуться назад", $"checkGroup={course}") }
//                 });
//                 await ctx.Bot.EditMessageText(
//                     ctx.Event.Chat,
//                     ctx.Event.MessageId,
//                     MessageBuilder.ShowSchedule(schedule, false),
//                     replyMarkup: keyboard,
//                     parseMode: ParseMode.Html,
//                     cancellationToken: token
//                 );
//             }
//         }
//     }
// }
