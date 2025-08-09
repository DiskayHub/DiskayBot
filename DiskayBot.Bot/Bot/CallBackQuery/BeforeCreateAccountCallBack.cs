using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.CallBackQuery;

public class BeforeCreateAccountCallBack : AbstractBotCallBack {
    private readonly RedisController _redis;

    public BeforeCreateAccountCallBack(RedisController redis) : base("beforeCreateAccount") {
        _redis = redis;
    }

    public override async Task ExecuteAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken, string? callBack) {
        var chat = update.CallbackQuery.Message.Chat;
        var messageId = update.CallbackQuery.Message.MessageId;
        
        var ChatId = chat.Id.ToString();
        
        await botClient.DeleteMessage(chat, messageId);
        
        try{
            var cashData = _redis.GetDataHash(ChatId);

            if (cashData != null){
                
                var hash = new HashEntry[] {
                    new HashEntry("group_id", callBack),
                };
                await _redis.SaveDataHash(chat.Id.ToString(), hash, TimeSpan.FromSeconds(100));
                
                var keyboard = GetKeyboard();
                
                await botClient.SendMessage(
                    chatId:chat, 
                    text: MessageBuilder.AdditionalInfoOffer(),
                    replyMarkup:  keyboard,
                    parseMode: ParseMode.Markdown
                );
            }
            else{
                await botClient.SendMessage(chat,  MessageBuilder.RegisterTimeOut(), ParseMode.Markdown);
            }
        }
        catch (Exception e){
            throw new Exception(e.Message);
        }
    }

    public override ReplyMarkup GetKeyboard() {
        var buttons = new[] {
            InlineKeyboardButton.WithCallbackData("Давай", "additionalInfo"),
            InlineKeyboardButton.WithCallbackData("Нет спасибо", "createAccountOffer")
        };
        
        var keyboard = new InlineKeyboardMarkup(buttons);
        return keyboard;
    }
}