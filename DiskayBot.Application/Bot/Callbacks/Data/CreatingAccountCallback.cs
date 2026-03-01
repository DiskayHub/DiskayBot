using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Redis;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Bot.Callbacks.Data;

[CallbackName("createAccount")]
public class CreatingAccountCallback : IBaseCommand {
    private readonly RedisController _redis;
    private readonly MemoryController _memoryController;

    public CreatingAccountCallback(RedisController redis, MemoryController memoryController) {
        _redis = redis;
        _memoryController = memoryController;
    }

    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        var callbackEvent = (CallbackQueryUserEvent)ctx.Event;
        await ctx.Bot.DeleteMessage(ctx.Event.Chat, ctx.Event.MessageId, cancellationToken: token);

        if (callbackEvent.Query == "yes") {
            var cash = await _redis.GetDataHash(ctx.Event.Chat.Id.ToString());
            if (cash != null) {
                var groupId = cash.FirstOrDefault(x => x.Name.ToString() == "group_id").Value;
                var request = await _memoryController.CreateUser(ctx.Event.UserId, ctx.Event.Username, groupId.ToString());
                if (request) {
                    await ctx.Bot.SendMessage(ctx.Event.Chat, $"Добро пожаловать, *{ctx.Event.Username}*!", ParseMode.Markdown, cancellationToken: token);
                }
                else {
                    await ctx.Bot.SendMessage(ctx.Event.Chat, "Diskay не может запомнить вас :(", ParseMode.Markdown, cancellationToken: token);
                }
                await _redis.DeleteData(ctx.Event.Chat.Id.ToString());
            }
            else {
                throw new TimeoutException();
            }
        }
        else {
            await ctx.Bot.SendMessage(ctx.Event.Chat, "Операция была отклонена.", ParseMode.Markdown, cancellationToken: token);
            await _redis.DeleteData(ctx.Event.Chat.Id.ToString());
        }
    }
}
