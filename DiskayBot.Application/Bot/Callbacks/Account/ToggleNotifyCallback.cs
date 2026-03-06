using System.Net;
using DiskayBot.API.Contracts.Users.UpdateUser;
using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.KeyBoard.Scripts;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Redis.Abstractions;
using Telegram.Bot;

namespace DiskayBot.Bot.Bot.Callbacks.Account;

[CallbackName("toggleNotify", AccessLevel.User)]
public class ToggleNotifyCallback : IBaseCommand {
    private readonly MemoryController _memoryController;
    private readonly IRedisController _redis;

    public ToggleNotifyCallback(MemoryController memoryController, IRedisController redis) {
        _memoryController = memoryController;
        _redis = redis;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var newNotify = !ctx.User!.notify;

        var request = await _memoryController.UpdateUser(ctx.Event.UserId, new UpdateUserRequest(
            group_id: null,
            sub_group: null,
            eng_group: null,
            prof_group: null,
            notify: newNotify
        ));

        if (request != HttpStatusCode.OK) {
            throw new Exception("Ошибка при обновлении настроек уведомлений");
        }

        await _redis.DeleteUser(ctx.Event.UserId.ToString());

        await ctx.Bot.EditMessageText(
            ctx.Event.Chat,
            ctx.Event.MessageId,
            "Настройки",
            replyMarkup: GlobalKeyboard.GetSettingsKeyboard(newNotify),
            cancellationToken: token
        );
    }
}
