using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiskayBot.Bot;

public record ServiceConfiguration {
    public string url { get; init; } = string.Empty;
}

public record ServicesConfiguration {
    public ServiceConfiguration DiskayMemory { get; init; } = new();
    public ServiceConfiguration Redis { get; init; } = new();
    public ServiceConfiguration ScheduleService { get; init; } = new();
}

public record Services {
    public ServicesConfiguration Default { get; init; } = new();
    public ServicesConfiguration Docker { get; init; } = new();
}
public class FileConfiguration {
    public Services Services { get; init; } = new();
}

public class Configuration {
    public ServicesConfiguration Services { get; init; } = new();

    public Configuration(ServicesConfiguration? services) {
        if (services == null) {
            throw new NullReferenceException($"{nameof(services)} not configured");
        }
        Services = services;
    }
}

public class Configurator {
    private readonly IConfiguration _configuration;
    private Configuration Configuration { get; init; }
    public Configurator(IConfiguration configuration, bool forDocker = false) {
        _configuration = configuration;
        if (forDocker) {
            var dockerConfiguration = _configuration.GetSection("Configuration").Get<FileConfiguration>().Services.Docker;
            Configuration = new Configuration(dockerConfiguration);
        }
        else {
            var defaultConfiguration = _configuration.GetSection("Configuration").Get<FileConfiguration>().Services.Default;
            Configuration = new Configuration(defaultConfiguration);
        }
    }

    public Configuration GetConfiguration() {
        return Configuration;
    }
}