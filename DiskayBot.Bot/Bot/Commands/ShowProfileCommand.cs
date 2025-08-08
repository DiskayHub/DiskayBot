using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Commands;

public class ShowProfileCommand : AbstractBotCommand {
    private readonly RedisController _redis;
    private readonly UserService _service;

    public ShowProfileCommand(RedisController redis, UserService service) : base("/show_profile")  {
        _redis = redis;
        _service = service;
    }
    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
        var username = update.Message.From.Username;
        var userId = update.Message.From.Id;
        var chat =  update.Message.Chat;

        try{
            var redis_request = await _redis.GetUser(username);

            if (redis_request != null){
                string result = MessageBuilder.ShowProfile(redis_request);
                await botClient.SendMessage(chat, result, ParseMode.Markdown);
            }
            else{
                var database_request = await _service.Authorization(userId);

                if (database_request != null){
                    string result = MessageBuilder.ShowProfile(database_request);
                    await botClient.SendMessage(chat, result, ParseMode.Markdown);
                    _redis.SaveUser(username, database_request);
                }
                else
                    await botClient.SendMessage(chat, MessageBuilder.NotRegistered(), ParseMode.Markdown);
            }
        }
        catch (HttpRequestException e){
            throw new ConnectionRefuseExeption("Ошибка подключения",  _service.Name);
        }
        catch (Exception e) {
            throw new Exception(e.Message);
        }
    }
}