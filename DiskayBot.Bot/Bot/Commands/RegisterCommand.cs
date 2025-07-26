using System.Net;
using DiskayBot.API.Contracts;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Commands;

public class RegisterCommand : AbstractBotCommand {
    private readonly RedisController _redis;
    private readonly BotService _service;

    public RegisterCommand(RedisController redis, BotService service) : base("/create_account") {
        _redis = redis;
        _service = service;
    }

    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
        var chat = update.Message.Chat;
        var userId = update.Message.From.Id;

        try{
            var request_redis = await _redis.GetUser(userId.ToString());

            if (request_redis == null){
                var userData_request = await _service.Authorization(userId);

                if (userData_request == null) {
                    var keyboard = await GetReplyMarkup();
                    await botClient.SendMessage(chat, MessageBuilder.CreateAccount(), ParseMode.Markdown);
                    await botClient.SendMessage(
                        chat,
                        "Выберите группу",
                        ParseMode.Markdown,
                        replyMarkup: keyboard
                    );   
                }
                else {
                    _redis.SaveUser(userId.ToString(), userData_request);
                    await botClient.SendMessage(chat, "Кажется вы уже авторизованы", ParseMode.Markdown);
                }
            }
            else {
                await botClient.SendMessage(chat, "Кажется вы уже авторизованы", ParseMode.Markdown);
            }
        }
        
        catch (Exception e){
            throw new Exception(e.Message);
        }
    }

    public async Task<ReplyMarkup?> GetReplyMarkup() {
        var groups = await _service.GetAllGroups();

        var keyboard_rows = groups.Select(item =>
            new[] { InlineKeyboardButton.WithCallbackData(item.name, $"group_{item.id}") }
        ).ToList();
        var keyboard = new InlineKeyboardMarkup(keyboard_rows);
        return keyboard;
    }
}
