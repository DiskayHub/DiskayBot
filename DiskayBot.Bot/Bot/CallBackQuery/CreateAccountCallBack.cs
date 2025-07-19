using DiskayBot.Bot.Abstractions;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Controllers;

public class CreateAccountCallBack : AbstractBotCallBack {
    private readonly RedisController _redis;

    public CreateAccountCallBack(RedisController redis) : base("createAccount") {
        _redis = redis;
    }
    
    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, 
        CancellationToken cancellationToken, string? query) {

        var chat = update.CallbackQuery.Message.Chat;
        var chat_id = chat.Id.ToString();
        
        try {
            var cash = await _redis.GetDataHash(chat_id);
            var group_id = cash.FirstOrDefault(x => x.Name.ToString() == "group_id").Value;

            if (group_id.HasValue){
                await botClient.SendMessage(chat, group_id.ToString(), ParseMode.Markdown);   
            }
            else {
                await botClient.SendMessage(chat, "Таймаут. Попробуйте ещё раз.", ParseMode.Markdown);   
            }
        }
        catch (Exception e){
            Console.WriteLine(e.GetType());
            Console.WriteLine(e.Message);
            throw;
        }
    }
}