using DiskayBot.Bot.Attributes;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Interfaces;
using DiskayBot.Bot.Messages;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.Commands;

[CommandName("/create_account")]
public class RegisterCommand : IBaseCommand {
    public async Task ExecuteAsync(BotContext ctx, CancellationToken token) {
        if (ctx.User == null) {
            var keyboard = new InlineKeyboardMarkup(new[] {
                InlineKeyboardButton.WithCallbackData("Продолжить", "chooseCourse")
            });
            await ctx.Bot.SendMessage(ctx.Event.Chat, MessageBuilder.CreateAccount(), ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: token);
        }
        else {
            await ctx.Bot.SendMessage(ctx.Event.Chat, "Кажется, вы уже авторизованы.", ParseMode.Markdown, cancellationToken: token);
        }
    }
}
