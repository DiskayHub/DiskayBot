using System.Net;
using DiskayBot.Infrastructure.Contracts.Users.UpdateUser;
using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Features.Account;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Telegram.Events;
using DiskayBot.Host.Abstractions;
using DiskayBot.Infrastructure.Redis;
using DiskayBot.Infrastructure.Redis.Abstractions;
using Telegram.Bot;

namespace DiskayBot.Host.Features.Account.Callbacks;

[CallbackName("changingGroup", AccessLevel.User)]
public class ChangingGroupCallback : IBaseCommand {
    private readonly MemoryController _memoryController;
    private readonly IRedisController _redis;

    public ChangingGroupCallback(MemoryController memoryController, IRedisController redis) {
        _memoryController = memoryController;
        _redis = redis;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var callbackEvent = (CallbackQueryUserEvent)ctx.Event;

        if (callbackEvent.Query != null) {
            var groupId = callbackEvent.Query;
            var requestBody = new UpdateUserRequest(
                group_id: Guid.Parse(groupId),
                eng_group: null,
                sub_group: null,
                prof_group: null,
                notify: null
            );
            var request = await _memoryController.UpdateUser(ctx.Event.UserId, requestBody);
            if (request == HttpStatusCode.OK) {
                await ctx.Bot.EditMessageText(
                    ctx.Event.Chat,
                    ctx.Event.MessageId,
                    "Группа была изменена",
                    cancellationToken: token
                );
                await _redis.DeleteUser(ctx.Event.UserId.ToString());
            }
            else {
                throw new Exception("Ошибка при обновлении группы пользователя");
            }
        }
    }
}
