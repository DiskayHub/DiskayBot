using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Redis;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

public class ShowProfileCommand : AbstractBotCommand {
    private readonly RedisController _redis;
    private readonly MemoryService _service;

    public ShowProfileCommand(RedisController redis, MemoryService service) : base("/show_profile")  {
        _redis = redis;
        _service = service;
    }
    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
        var userId = update.Message.From.Id;
        var chat =  update.Message.Chat;

        try{
            var redis_request = await _redis.GetUser(userId.ToString());

            if (redis_request != null){
                string result = MessageBuilder.ShowProfile(redis_request);
                await botClient.SendMessage(chat, result, ParseMode.Markdown);
            }
            else {
                var database_request = await _service.Authorization(userId);

                if (database_request != null){
                    string result = MessageBuilder.ShowProfile(database_request);
                    await botClient.SendMessage(chat, result, ParseMode.Markdown);
                    _redis.SaveUser(userId.ToString(), database_request);
                }
                else
                    await botClient.SendMessage(chat, MessageBuilder.NotRegistered(), ParseMode.Markdown);
            }
        }
        catch (Exception e) {
            throw new Exception(e.Message);
        }
    }
}