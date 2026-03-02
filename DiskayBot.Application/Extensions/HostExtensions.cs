using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace DiskayBot.Bot.Extensions;

public static class HostExtensions {
    public static IHostBuilder AddLogging(this IHostBuilder hostBuilder, string workDirectory) {
        return hostBuilder.ConfigureLogging(logging => {
            logging.ClearProviders();
        })
        .UseSerilog((ctx, lc) => lc
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(log => log.Level >= LogEventLevel.Error)
                .WriteTo.File($"{workDirectory}/logs/errors/errors.log")
            )
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(log => log.Level == LogEventLevel.Information)
                .WriteTo.File(
                    path: $"{workDirectory}/logs/info/.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
            )

            .WriteTo.File(
                path: $"{workDirectory}/logs/all/.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
        );
    }

    public static IHostBuilder AddConfiguration(this IHostBuilder hostBuilder, bool isDocker) {
        return hostBuilder.ConfigureAppConfiguration((context, config) => {
                if (!isDocker) {
                    // Если бот запущен не в Docker
                    Console.WriteLine("DEFAULT CONFIGURATION");
                    var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
                    config.SetBasePath(path);
                }
                else {
                    Console.WriteLine("DOCKER CONFIGURATION");
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