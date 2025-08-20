using DiskayBot.Bot.Interfaces;
using Telegram.Bot.Types;

namespace DiskayBot.Bot.Events.Data;

public record GroupChosenEvent(
    Chat Chat,
    string GroupId
);