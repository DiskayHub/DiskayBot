namespace DiskayBot.Infrastructure.Contracts.Service;

public class PingResponse {
    public string serviceName { get; }
    public string serviceStatus { get; }
    public string? dataBaseStatus { get; }

    public PingResponse(string serviceName, string serviceStatus, string? dataBaseStatus = null) {
        this.serviceName = serviceName;
        this.serviceStatus = serviceStatus;
        this.dataBaseStatus = dataBaseStatus;
    }

    public static PingResponse CreateDefault(string serviceName) {
        return new PingResponse(serviceName, "INACTIVE");
    }
};