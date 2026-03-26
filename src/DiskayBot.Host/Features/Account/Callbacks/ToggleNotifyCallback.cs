using System.Net;
using DiskayBot.Infrastructure.Contracts.Users.UpdateUser;
using DiskayBot.Host.Telegram.Attributes;
using DiskayBot.Host.Features.Account;
using DiskayBot.Host.Presentation.Keyboards.Scripts;
using DiskayBot.Host.Telegram.DTOs;
using DiskayBot.Host.Abstractions;
using DiskayBot.Infrastructure.Redis.Abstractions;
using Telegram.Bot;

namespace DiskayBot.Host.Features.Account.Callbacks;

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
