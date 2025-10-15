namespace DiskayBot.API.Contracts.Users.GetUser;

public record TelegramUser(
    long user_id,
    string username,
    string group_name
);