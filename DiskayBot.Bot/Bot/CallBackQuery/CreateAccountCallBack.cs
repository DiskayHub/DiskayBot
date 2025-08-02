using System.Net;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.CallBackQuery;

public class CreateAccountCallBack : AbstractBotCallBack {
    private readonly RedisController _redis;
    private readonly MemoryService _service;

    public CreateAccountCallBack(RedisController redis, MemoryService service) : base("createAccount") {
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
                var GroupId = cash.FirstOrDefault(x => x.Name.ToString() == "group_id").Value;
                
                var UserId = update.CallbackQuery.From.Id;
                var Username =  update.CallbackQuery.From.Username;
                
                if (GroupId.HasValue){
                    var request = await _service.Registration(UserId, Username, GroupId.ToString());
                    if (request == HttpStatusCode.OK){
                        botClient.SendMessage(ChatId, $"Добро пожаловать, {Username}!", ParseMode.Markdown);
                    }
                    else {
                        botClient.SendMessage(ChatId, $"Ошибка при отправке данных", ParseMode.Markdown);
                    }
                }
                else {
                    await botClient.SendMessage(chat, "Таймаут. Попробуйте ещё раз.", ParseMode.Markdown);   
                }
            }
            catch (Exception e){
                Console.WriteLine(e.GetType());
                throw new Exception(e.Message);
            }
        }
        else {
            await botClient.SendMessage(chat, "Операция была отклонена. ", ParseMode.Markdown);
            _redis.DeleteData(ChatId);
        }
    }
}