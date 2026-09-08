namespace DiskayBot.API.Clients.Options;

public class ScheduleClientOptions {
    public string url { get; set; }
    public bool authEnabled { get; set; }
    public string? login { get; set; }
    public string? password { get; set; }
}
