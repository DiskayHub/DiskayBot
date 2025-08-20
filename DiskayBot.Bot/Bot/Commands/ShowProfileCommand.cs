using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Bot.Messages;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;

namespace DiskayBot.Bot.Bot.Commands;

public class ShowProfileCommand : BotCommand {
    private readonly RedisController _redis;
    private readonly UserService _userService;

    public ShowProfileCommand(string name, RedisController redis, UserService userService) : base(name)  {
        _redis = redis;
        _userService = userService;
    }
    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        try{
            var redisRequest = await _redis.GetUser(evt.Username);

            if (redisRequest != null){
                string result = MessageBuilder.ShowProfile(redisRequest);
                await bot.SendMessage(evt.Chat, result, ParseMode.Markdown);
            }
            else{
                var databaseRequest = await _userService.Authorization(evt.UserId);

                if (databaseRequest != null){
                    string result = MessageBuilder.ShowProfile(databaseRequest);

                    await bot.SendMessage(evt.Chat, result, ParseMode.Markdown);
                    await _redis.SaveUser(evt.Username, databaseRequest);
                }
                else
                    await bot.SendMessage(evt.Chat, MessageBuilder.NotRegistered(), ParseMode.Markdown);
            }
        }
        catch (Exception e){
            
        }
    }
}