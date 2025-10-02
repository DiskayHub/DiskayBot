using System.Net;
using DiskayBot.API.Contracts.Users.UpdateUser;
using DiskayBot.API.Services;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Exeptions;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = DiskayBot.Bot.Abstractions.BotCommand;

namespace DiskayBot.Bot.Bot.CallBacks.Account;

public class ChangingGroupCallback : BotCommand {
    private readonly RedisController _redis;
    private readonly UserController _userController;
    private readonly UserClient _userClient;
    
    public ChangingGroupCallback(string name, RedisController redis, UserController userController, UserClient userClient) : base(name) {
        _redis = redis;
        _userController = userController;
        _userClient = userClient;
    }

    public override async Task ExecuteAsync(ITelegramBotClient bot, CancellationToken token, UserEvent evt) {
        var user = await _userController.GetUserData(evt.UserId);
        if (user != null) {
            var data = await _redis.GetDataHash(evt.Chat.Id.ToString());
            if (data != null) {
                var groupId = data.FirstOrDefault(x => x.Name.ToString() == "group_id").Value;
                var requestBody = new UpdateUserRequest(
                    group_id: Guid.Parse(groupId),
                    eng_group: null,
                    sub_group: null,
                    prof_group: null
                );
                var request = await _userClient.UpdateUser(evt.UserId, requestBody);
                if (request == HttpStatusCode.OK) {
                    await bot.EditMessageText(
                        evt.Chat,
                        evt.MessageId,
                        "Группа была изменена"
                    );
                    await _redis.DeleteData(evt.UserId.ToString());
                    await _redis.DeleteUser(evt.UserId.ToString());
                }
                else {
                    throw new Exception();   
                }
            }
            else {
                throw new Exception();
            }
        }
        else {
            throw new NotAuthorizatedExeption();
        }
    }
}