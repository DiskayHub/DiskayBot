using DiskayBot.API.Contracts;
using DiskayBot.API.Exeptions;
using DiskayBot.API.Interfaces;
using DiskayBot.API.Modules;
using DiskayBot.Services.ScheduleService.Components;
using DiskayBot.Services.ScheduleService.Data;
using DiskayBot.Services.ScheduleService.Events;
using DiskayBot.Services.ScheduleService.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiskayBot.Services.ScheduleService;

public class ScheduleService : IScheduleServiceEvents, IScheduleService {
    private readonly IScheduleClient _client;
    private readonly ILogger<ScheduleService> _logger;
    private readonly List<string> _allGroups;
    public readonly IScheduleAnalyser Analyser;
    public readonly IScheduleController Controller;
    public WeekSchedule Schedule {get; set; }
    public event Action<UpdateScheduleEvent> OnScheduleUpdated;
    
    public ScheduleService(IScheduleClient client, ILogger<ScheduleService> logger, ILoggerFactory loggerFactory) {
        _client = client;
        _allGroups = [
            "ИТ25-11", "ИТ25-12", "ИТ25-13", "ИТ25-14",
            "ИТ24-11", "ИТ24-12", "ИТ24-13", "ИТ24-14",
            "ИТ23-11", "ИТ23-12", "ИТ23-13",
            "ИТ22-11", "ИТ22-12"
        ];
        _logger = logger;
        Schedule = new WeekSchedule();
        Analyser = new ScheduleAnalyser(this, loggerFactory.CreateLogger<ScheduleAnalyser>());
        Controller = new ScheduleController(this, loggerFactory.CreateLogger<ScheduleController>());
    }

    private async Task UpdateSchedules() {
        _logger.LogInformation("Обновление данных о расписании...");
        var previosSchedule = new WeekSchedule(Schedule);
        foreach (var group in _allGroups) {
            var schedule = await _client.GetActualScheduleWeek(group);
            if (schedule != null) {
                Schedule.WeekPeriod = schedule.WeekPeriod;
                Schedule.GroupsSchedule[group] = schedule.Schedule;
            }
        }
        _logger.LogInformation($"Обновление завершено. Всего групп: {Schedule.GroupsSchedule.Count}");

        var scheduleUpdateEvent = new WeekSchedule(
            weekPeriod: TimeHelper.GetActualWeekPeriod(),
            groupsSchedule: Schedule.GroupsSchedule
        );
        
        OnScheduleUpdated.Invoke(new  UpdateScheduleEvent(previosSchedule, scheduleUpdateEvent));
    }
    public List<DaySchedule>? GetWeekSchedule(string groupName) {
        _logger.LogInformation($"Получаю расписание для группы: {groupName}...");
        Schedule.GroupsSchedule.TryGetValue(groupName, out var schedule);
        return schedule;
    }

    public async Task Run(TimeSpan delay, CancellationToken? token = default) {
        Analyser.Listen();
        _logger.LogInformation("Запуск сервиса ScheduleService");
        
        var timer = new PeriodicTimer(delay);
        try {
            await UpdateSchedules();
            if (await timer.WaitForNextTickAsync()) {
                await UpdateSchedules();
            }
        }
        catch (HttpRequestException ex) {
            _logger.LogError(ex, "Ошибка при отправке запроса к Schedule API!");
        }

        catch (ConnectionRefuseExeption ex) {
            _logger.LogError(ex, "Потеряно соединение с сервером!");
        }

        catch (TaskCanceledException ex) {
            _logger.LogError(ex, "Истекло время ожидания запроса на сервер!");
        }

        catch (Exception ex) {
            _logger.LogError(ex, "Неизвестная ошибка при отправке запроса к Schedule API!");
        }
    }

    public WeekSchedule WeekSchedule { get; set; }

    public void RaiseOnScheduleUpdated(UpdateScheduleEvent e) {
        OnScheduleUpdated.Invoke(e);
    }
}