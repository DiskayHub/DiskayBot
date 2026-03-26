namespace DiskayBot.Host.Telegram.Exceptions;

public class NotAuthorizatedExсeption : Exception {
    public NotAuthorizatedExсeption(string message = "User is not authorized") : base(message) {
        
    }
}