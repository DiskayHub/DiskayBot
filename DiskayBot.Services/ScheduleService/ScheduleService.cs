using DiskayBot.API.Contracts;
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

    private UpdateScheduleEvent CreateDaysSchedule(
        bool differentWeeks = false,
        bool differentGroups = false,
        bool differentItems = false) {
        // === 1. Определяем даты недели ===
        var baseStart = new DateOnly(2025, 10, 6);
        var baseEnd = new DateOnly(2025, 10, 12);

        var currentStart = differentWeeks ? baseStart.AddDays(7) : baseStart;
        var currentEnd = differentWeeks ? baseEnd.AddDays(7) : baseEnd;

        // === 2. Определяем группы ===
        var baseGroups = new[] { "ИТ24-11", "ИТ24-12" };
        var currentGroups = differentGroups
            ? new[] { "ИТ24-13", "ИТ24-14" }
            : baseGroups;

        // === 3. Создаём предметы (DayItem) ===
        List<DayItem> CreateDayItems(bool different) {
            if (!different) {
                return new List<DayItem> {
                    new("Математика", "Лекция", "101", new TimeOnly(9, 0), new TimeOnly(10, 30),
                        new List<SubGroupItem> { new("Подгруппа A", "Описание", "101", "1") })
                };
            }

            // отличающиеся предметы
            return new List<DayItem> {
                new("Физика", "Практика", "202", new TimeOnly(11, 0), new TimeOnly(12, 30),
                    new List<SubGroupItem> { new("Подгруппа B", "Описание", "202", "2") })
            };
        }

        // === 4. Создаём расписание на неделю ===
        List<DaySchedule> CreateWeekDays(string group, bool different) {
            return new List<DaySchedule> {
                new(
                    new DateOnly(2025, 10, 6),
                    group,
                    CreateDayItems(different)
                )
            };
        }

        // === 5. Собираем WeekSchedule для предыдущей недели ===
        var prevWeek = new WeekSchedule(
            new TimePeriod(baseStart, baseEnd),
            baseGroups.ToDictionary(
                g => g,
                g => CreateWeekDays(g, false)
            )
        );

        // === 6. И для текущей недели ===
        var currWeek = new WeekSchedule(
            new TimePeriod(currentStart, currentEnd),
            currentGroups.ToDictionary(
                g => g,
                g => CreateWeekDays(g, differentItems)
            )
        );

        // === 7. Формируем событие ===
        return new UpdateScheduleEvent(prevWeek, currWeek);
    }

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
        _logger.LogDebug("Обновление данных о расписании...");

        var scheduleUpdateEvent = CreateDaysSchedule(
            
        );
        
        OnScheduleUpdated.Invoke(scheduleUpdateEvent);
    }
    public List<DaySchedule>? GetWeekSchedule(string groupName) {
        _logger.LogInformation($"Получаю расписание для группы: {groupName}...");
        Schedule.GroupsSchedule.TryGetValue(groupName, out var schedule);
        return schedule;
    }

    public async Task Run(TimeSpan delay, CancellationToken? token = default) {
        Analyser.Listen();
        _logger.LogInformation("Запуск сервиса ScheduleService..");
        
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
    }

    public WeekSchedule WeekSchedule { get; set; }

    public void RaiseOnScheduleUpdated(UpdateScheduleEvent e) {
        OnScheduleUpdated.Invoke(e);
    }
}