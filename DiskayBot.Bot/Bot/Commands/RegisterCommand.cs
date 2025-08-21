using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Bot.KeyBoard;
using DiskayBot.Bot.Bot.KeyBoard.Scripts;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;

namespace DiskayBot.Bot.Bot.Commands;

public class RegisterCommand : BotCommand {
    private readonly RedisController _redis;
    private readonly UserService _userService;
    private readonly GlobalKeyboard _globalKeyboard;

    public RegisterCommand(string name, RedisController redis, UserService userService, GlobalKeyboard globalKeyboard) : base(name) {
        _redis = redis;
        _userService = userService;
        _globalKeyboard = globalKeyboard;
        
        _globalKeyboard.BindKeyboard(new UserKeyboard(
            name, new List<UserButton>() {
                new UserButton("Продолжить", "continue=showAllCourses")
            })
        );
    }

    public override async Task ExecuteAsync(ITelegramBotClient botClient, CancellationToken token, UserEvent evt) {
        
        try{
            var requestRedisUser = await _redis.GetUser(evt.Username);

            if (requestRedisUser == null){
                var requestRedisSession = await _redis.GetDataHash(evt.UserId.ToString());
                if (requestRedisSession == null){
                    var userDataRequest = await _userService.Authorization(evt.UserId);

                    if (userDataRequest == null){
                        await botClient.SendMessage(
                            evt.Chat,
                            MessageBuilder.CreateAccount(),
                            ParseMode.Markdown,
                            replyMarkup: _globalKeyboard.GetKeyBoard(Name)!.GetInlineKeyboard()
                        );
                    }
                    else{
                        await botClient.SendMessage(evt.Chat, "Кажется вы уже авторизованы", ParseMode.Markdown);
                        await _redis.SaveUser(evt.Username, userDataRequest);
                    }
                }
                else {
                    await botClient.SendMessage(evt.Chat, "Вы не завершили сессию. \nЗакончитете её, либо дождитесь таймаута.", 
                        ParseMode.Markdown);
                }
            }
            else{
                await botClient.SendMessage(evt.Chat, "Кажется вы уже авторизованы", ParseMode.Markdown);
            }
        }

        catch (HttpRequestException e){
            throw new ConnectionRefuseExeption("Ошибка при подключении", _userService.Name);
        }
        
        catch (Exception e){
            throw new Exception(e.Message);
        }
    }
}