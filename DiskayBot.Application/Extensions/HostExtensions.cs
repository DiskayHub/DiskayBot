using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace DiskayBot.Bot.Extensions;

public static class HostExtensions {
    private const string OutputTemplate =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

    public static IHostBuilder AddLogging(this IHostBuilder hostBuilder, string workDirectory) {
        return hostBuilder.ConfigureLogging(logging => {
            logging.ClearProviders();
        })
        // Уровни берутся из секции "Serilog" конфигурации, их можно переопределить переменными окружения,
        // например Serilog__MinimumLevel__Default=Debug
        .UseSerilog((ctx, lc) => lc
            .ReadFrom.Configuration(ctx.Configuration)
            .WriteTo.Console(outputTemplate: OutputTemplate)
            .WriteTo.File(
                path: $"{workDirectory}/logs/all/.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: OutputTemplate
            )
            .WriteTo.File(
                path: $"{workDirectory}/logs/errors/.log",
                restrictedToMinimumLevel: LogEventLevel.Error,
                rollingInterval: RollingInterval.Day,
                outputTemplate: OutputTemplate
            )
        );
    }

    public static IHostBuilder AddConfiguration(this IHostBuilder hostBuilder, bool isDocker) {
        return hostBuilder.ConfigureAppConfiguration((context, config) => {
                if (!isDocker) {
                    var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
                    config.SetBasePath(path);
                }

                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                if (isDocker) {
                    config.AddJsonFile("appsettings.Docker.json", optional: true, reloadOnChange: true);
                }
                config.AddEnvironmentVariables();
            }
        );
    }
}
