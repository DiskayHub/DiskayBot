using Telegram.Bot.Exceptions;

namespace DiskayBot.Bot.Bot.Exeptions;

public static class ApiRequestExceptionExtensions {
    // Telegram отвечает так, когда новый текст и клавиатура совпадают с текущими — это не ошибка
    public static bool IsMessageNotModified(this ApiRequestException exception) {
        return exception.Message.Contains("message is not modified");
    }
}
