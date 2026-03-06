using DiskayBot.API.Interfaces;
using DiskayBot.Bot.ScheduleService.Events;
using DiskayBot.Bot.ScheduleService.Options;
using DiskayBot.Redis.Abstractions;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiskayBot.Bot.ScheduleService.Worker;

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
            var freshWeekSchedule = await _scheduleClient.GetActualScheduleWeek(group);
            if (freshWeekSchedule != null) {
                foreach (var freshDaySchedule in freshWeekSchedule.Schedule) {
                    var pastScheduleIsActual = await _redis.CheckScheduleEquals(freshDaySchedule);
                    if (pastScheduleIsActual == false) {
                        await _mediator.Publish(new ScheduleUpdatedEvent(freshDaySchedule));
                        await _redis.SaveSchedule(freshDaySchedule);
                    }
                }   
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogDebug($"Таймаут запросов: {_options.updateTimeout}");
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.updateTimeout));
        try {
            do {
                _logger.LogDebug("Обновление расписания...");
                await UpdateSchedule();
                _logger.LogInformation("Расписание обновленно");
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (Exception ex) {
            _logger.LogError($"Необработанное исключение при обновлении расписания: {ex.Message}");
        }
    }
}