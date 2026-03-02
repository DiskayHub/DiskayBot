using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Callbacks.Schedule;

// [CallbackName("updateSchedule", AccessLevel.User)]
// public class UpdateScheduleCallback : IBaseCommand {
//     private readonly IScheduleController _scheduleService;
//
//     public UpdateScheduleCallback(IScheduleController scheduleService) {
//         _scheduleService = scheduleService;
//     }
//
//     public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
//         var callbackEvent = (CallbackQueryUserEvent)ctx.Event;
//         var schedule = _scheduleService.GetActualSchedule($"ИТ{ctx.User!.group_name}");
//         if (schedule != null) {
//             try {
//                 var keyboard = new InlineKeyboardMarkup(new[] {
//                     InlineKeyboardButton.WithCallbackData("Обновить 💫", "updateSchedule")
//                 });
//                 await ctx.Bot.EditMessageText(
//                     ctx.Event.Chat,
//                     ctx.Event.MessageId,
//                     MessageBuilder.ShowSchedule(schedule),
//                     replyMarkup: keyboard,
//                     parseMode: ParseMode.Html,
//                     cancellationToken: token
//                 );
//             }
//             catch (ApiRequestException) {
//                 await ctx.Bot.AnswerCallbackQuery(callbackEvent.Id, cancellationToken: token);
//             }
//         }
//     }
// }
