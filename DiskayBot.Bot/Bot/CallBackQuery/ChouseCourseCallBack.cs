using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Controllers;

public class ChouseCourseCallBack : AbstractBotCallBack {
    private readonly RedisController _redis;
    private readonly UserService _service;

    public ChouseCourseCallBack(RedisController redis, UserService service) : base("course") {
        _redis = redis;
        _service = service;
    }
    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken, string? callBack) {
        var chat =  update.CallbackQuery.Message.Chat;
        var userId = update.CallbackQuery.From.Id;

        try{
            var keyboard = await GetReplyMarkup(int.Parse(callBack));
            await botClient.SendMessage(
                chat,
                "Выберите группу",
                ParseMode.Markdown,
                replyMarkup: keyboard
            );
        }
        catch (Exception e){
            throw new Exception(e.Message);
        }
    }

    public async Task<ReplyMarkup?> GetReplyMarkup(int course) {
        var courseGroups = await _service.GetCourseGroups(course);
        
        courseGroups = courseGroups.OrderBy(c => {
            var parts = c.name.Split('-');
            return int.Parse(parts[1]);
        }).ToList();
        
        var keyboardRows = courseGroups.Select(group =>
            new[] { InlineKeyboardButton.WithCallbackData(group.name, $"group_{group.id}") }
        ).ToList();
        var keyboard = new InlineKeyboardMarkup(keyboardRows);
        return keyboard;
    }
}