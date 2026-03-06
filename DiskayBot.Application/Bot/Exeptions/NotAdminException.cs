namespace DiskayBot.Bot.Bot.Exeptions;

public class NotAdminException : Exception {
    public NotAdminException() : base("Admin access required") { }
}
