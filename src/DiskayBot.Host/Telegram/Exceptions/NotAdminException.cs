namespace DiskayBot.Host.Telegram.Exceptions;

public class NotAdminException : Exception {
    public NotAdminException() : base("Admin access required") { }
}
