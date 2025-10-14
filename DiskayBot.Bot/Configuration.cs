using Microsoft.Extensions.Configuration;

namespace DiskayBot.Bot;

public record ServiceConfiguration {
    public string url { get; init; } = string.Empty;
}

public record Services {
    public ServiceConfiguration DiskayMemory { get; init; } = new();
    public ServiceConfiguration Redis { get; init; } = new();
}
public class Configuration {
    public Services Services { get; init; } = new();
}