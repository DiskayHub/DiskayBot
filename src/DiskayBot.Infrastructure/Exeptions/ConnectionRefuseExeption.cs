namespace DiskayBot.Infrastructure.Exeptions;

public class ConnectionRefuseExeption : Exception {
    public string ServiceName { get; }
    public ConnectionRefuseExeption(string service_name, string message = "Ошибка при отправке запроса") : base(message) {
         ServiceName = service_name;
    }
}