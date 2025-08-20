using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Interfaces;
using Telegram.Bot;

namespace DiskayBot.Bot.Events.Data;

public record ShowGroupsEvent (
    ITelegramBotClient Bot,
    CallbackQueryUserEvent UserEvent,
    short Course,
    string TextMessage,
    string NextCallBack
);