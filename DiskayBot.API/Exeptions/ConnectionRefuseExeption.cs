namespace DiskayBot.Bot.Bot.Exeptions;

public class ConnectionRefuseExeption : Exception {
    public string serviceName { get; }
    public ConnectionRefuseExeption(string message, string service_name) : base(message) {
         serviceName = service_name;
    }
}