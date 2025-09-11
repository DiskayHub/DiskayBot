namespace DiskayBot.Bot.Bot.Exeptions;

public class NotAuthorizatedExeption : Exception {
    public NotAuthorizatedExeption(string message = "User is not authorized") : base(message) {
        
    }
}