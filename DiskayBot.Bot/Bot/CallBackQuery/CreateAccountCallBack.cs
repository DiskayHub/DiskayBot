using System.Net;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.CallBackQuery;

public class CreateAccountCallBack : AbstractBotCallBack {
    private readonly RedisController _redis;
    private readonly UserService _service;

    public CreateAccountCallBack(RedisController redis, UserService service) : base("createAccount") {
        _redis = redis;
        _service = service;
    }
    
    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, 
        CancellationToken cancellationToken, string? query) {

        var chat = update.CallbackQuery.Message.Chat;
        var messageId = update.CallbackQuery.Message.MessageId;
        
        var ChatId = chat.Id.ToString();
        
        await botClient.DeleteMessage(chat, messageId);

        if (query == "yes"){
            try {
                var cash = await _redis.GetDataHash(ChatId);
                
                if (cash != null){
                    var GroupId = cash.FirstOrDefault(x => x.Name.ToString() == "group_id").Value;
                    var UserId = update.CallbackQuery.From.Id;
                    var Username =  update.CallbackQuery.From.Username;
                    
                    var request = await _service.Registration(UserId, Username, GroupId.ToString());
                    if (request == HttpStatusCode.OK){
                        await botClient.SendMessage(ChatId, $"Добро пожаловать, {Username}!", ParseMode.Markdown);
                        await _redis.DeleteData(ChatId);
                    }
                    else {
                        await botClient.SendMessage(ChatId, $"Diskay не может запомнить вас :(", ParseMode.Markdown);
                    }
                }
                else {
                    await botClient.SendMessage(chat, MessageBuilder.RegisterTimeOut(), ParseMode.Markdown);   
                }
            }
            catch (Exception e){
                Console.WriteLine(e.GetType());
                throw new Exception(e.Message);
            }
        }
        else {
            await botClient.SendMessage(chat, "Операция была отклонена. ", ParseMode.Markdown);
            await _redis.DeleteData(ChatId);
        }
    }
}