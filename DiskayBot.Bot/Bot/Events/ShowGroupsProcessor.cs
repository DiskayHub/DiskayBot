using System.Data;
using DiskayBot.API.Services;
using DiskayBot.Bot.Events.Data;
using DiskayBot.Bot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Events;

public class ShowGroupsProcessor {
    private readonly UserService _userService;
    public string Name { get; }
    public ShowGroupsProcessor(UserService userService) {
        _userService = userService;
        Name = "";
    }

    public async Task HandleAsync(ShowGroupsEvent evt, CancellationToken cancellationToken = default) {
        var bot = evt.Bot;
        
        if (evt.UserEvent.Query != null){
            var keyboard = await GetReplyMarkup(evt.Course, evt.NextCallBack);
            
            await bot.EditMessageText(
                chatId: evt.UserEvent.Chat,
                messageId: evt.UserEvent.MessageId,
                text: evt.TextMessage,
                parseMode: ParseMode.Markdown,
                replyMarkup: keyboard
            );
        }
        else{
            throw new NoNullAllowedException();
        }
    }
    
    public async Task<InlineKeyboardMarkup?> GetReplyMarkup(short course, string nextCallBack = "group") {
        var courseGroups = await _userService.GetCourseGroups(course);
        
        courseGroups = courseGroups.OrderBy(c => {
            var parts = c.name.Split('-');
            return int.Parse(parts[1]);
        }).ToList();
        
        var keyboardRows = courseGroups.Select(group =>
            new[] { InlineKeyboardButton.WithCallbackData(group.name, $"{nextCallBack}_{group.id}") }
        ).ToList();
        var keyboard = new InlineKeyboardMarkup(keyboardRows);
        return keyboard;
    }
}