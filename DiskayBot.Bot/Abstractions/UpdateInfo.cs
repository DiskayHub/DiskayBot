using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Bot.Abstractions;

public record UpdateInfo(
    UpdateType Type,
    string Username,
    Chat Chat,
    long UserId,
    MessageId MessageId
);