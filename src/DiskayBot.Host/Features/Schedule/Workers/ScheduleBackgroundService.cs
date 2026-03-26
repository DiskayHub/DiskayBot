using DiskayBot.Infrastructure.Interfaces;
using DiskayBot.Host.Features.Schedule.Events;
using DiskayBot.Host.Features.Schedule.Options;
using DiskayBot.Infrastructure.Redis.Abstractions;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiskayBot.Host.Features.Schedule.Workers;

public class ScheduleBackgroundService : BackgroundService {
    private readonly IScheduleClient  _scheduleClient;
    private readonly IRedisController _redis;
    private readonly IMediator _mediator;
    private readonly ScheduleServiceOptions _options;
    private readonly ILogger<ScheduleBackgroundService> _logger;
    
    public ScheduleBackgroundService(IMediator mediator, IScheduleClient scheduleClient, IRedisController redis, IOptions<ScheduleServiceOptions> options, ILogger<ScheduleBackgroundService> logger) {
        _mediator =  mediator;
        _scheduleClient = scheduleClient;
        _redis = redis;
        _options = options.Value;
        _logger = logger;
    }
    private async Task UpdateSchedule() {
        foreach (var group in _options.allGroups) {
            try {
                var freshWeekSchedule = await _scheduleClient.GetCurrentWeekSchedule(group);
                if (freshWeekSchedule != null) {
                    foreach (var freshDaySchedule in freshWeekSchedule.Schedule) {
                        var pastScheduleIsActual = await _redis.CheckScheduleEquals(freshDaySchedule);
                        if (pastScheduleIsActual == false) {
                            await _mediator.Publish(new ScheduleUpdatedEvent(freshDaySchedule));
                            await _redis.SaveSchedule(freshDaySchedule);
                        }
                        else {
                            await _redis.SetScheduleDefaultExpire(freshDaySchedule);
                        }
                    }
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Ошибка обновления расписания для группы '{Group}'", group);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogDebug($"Таймаут запросов: {_options.updateTimeout}");
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.updateTimeout));
        do {
            _logger.LogDebug("Обновление расписания...");
            await UpdateSchedule();
            _logger.LogInformation("Расписание обновленно");
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
