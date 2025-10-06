using DiskayBot.API.Clients;
using DiskayBot.API.Contracts;
using DiskayBot.API.Interfaces;
using DiskayBot.API.Modules;
using DiskayBot.Services.ScheduleService;
using DiskayBot.Services.ScheduleService.Data;
using DiskayBot.Services.ScheduleService.Events;
using DiskayBot.Services.ScheduleService.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DiskayBot.Tests.Services;

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

public class ScheduleAnalyserTests {
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
        List<DayItem> CreateDayItems(bool different)
        {
            if (!different)
            {
                return new List<DayItem>
                {
                    new("Математика", "Лекция", "101", new TimeOnly(9, 0), new TimeOnly(10, 30),
                        new List<SubGroupItem> { new("Подгруппа A", "Описание", "101", "1") })
                };
            }

            // отличающиеся предметы
            return new List<DayItem>
            {
                new("Физика", "Практика", "202", new TimeOnly(11, 0), new TimeOnly(12, 30),
                    new List<SubGroupItem> { new("Подгруппа B", "Описание", "202", "2") })
            };
        }

        // === 4. Создаём расписание на неделю ===
        List<DaySchedule> CreateWeekDays(string group, bool different)
        {
            return new List<DaySchedule>
            {
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

    
    [Fact]
    public void Listen_ShouldSubscribeToEvent() {
        // Arrange
        var mockEvents = new Mock<IScheduleServiceEvents>();
        var mockLogger = new Mock<ILogger<ScheduleAnalyser>>();

        var analyser = new ScheduleAnalyser(mockEvents.Object, mockLogger.Object);

        // Act
        analyser.Listen();

        // Assert
        mockEvents.VerifyAdd(e => e.OnScheduleUpdated += It.IsAny<Action<UpdateScheduleEvent>>(), Times.Once);
    }
    
    [Fact]
    public void Analyse_ShouldBeCalled_WhenEventRaised() {
        // Arrange
        var mockEvents = new Mock<IScheduleServiceEvents>();
        var mockLogger = new Mock<ILogger<ScheduleAnalyser>>();
        var analyser = new ScheduleAnalyser(mockEvents.Object, mockLogger.Object);
        analyser.Listen();

        var updateEvent = new UpdateScheduleEvent(
            previosWeekSchedule: new WeekSchedule(
                new TimePeriod(new DateOnly(2025, 10, 6), new DateOnly(2025, 10, 12)),
                new Dictionary<string, List<DaySchedule>>()
            ),
            currentWeekSchedule: new WeekSchedule(
                new TimePeriod(new DateOnly(2025, 10, 6), new DateOnly(2025, 10, 12)),
                new Dictionary<string, List<DaySchedule>>()
            )
        );

        // Act — вызываем событие у мока
        mockEvents.Raise(e => e.OnScheduleUpdated += null, updateEvent);
    }
    
    [Fact]
    public void WhenNewSheduleAppears_ShouldInvokeEvent() {
        var mockEvent = new Mock<IScheduleServiceEvents>();
        var mockLogger = new Mock<ILogger<ScheduleAnalyser>>();
        
        var analyser = new ScheduleAnalyser(mockEvent.Object, mockLogger.Object);
        analyser.Listen();
        
        bool newWeekEventCalled = false;
        analyser.NewWeekScheduleAppear += () => {
            newWeekEventCalled = true;
            return Task.CompletedTask;
        };

        var updateEvent = CreateDaysSchedule(
            differentWeeks: true
        );
        
        mockEvent.Raise(e => e.OnScheduleUpdated += null, updateEvent);
        Assert.True(newWeekEventCalled, "Событие NewWeekScheduleAppear должно было вызваться");
    }
    
    [Fact] void WhenScheduleChanged_ShouldInvokeEvent() {
        var mockEvent = new Mock<IScheduleServiceEvents>();
        var mockLogger = new Mock<ILogger<ScheduleAnalyser>>();
        
        var analyser = new ScheduleAnalyser(mockEvent.Object, mockLogger.Object);
        bool changeWeekEventCalled = false;
        analyser.ScheduleChanged += () => {
            changeWeekEventCalled = true;
            return Task.CompletedTask;
        };
        
        analyser.Listen();

        var updateEvent = CreateDaysSchedule(
            differentWeeks: false,
            differentGroups: false,
            differentItems: true
        );
        
        mockEvent.Raise(e => e.OnScheduleUpdated += null, updateEvent);
        Assert.True(changeWeekEventCalled, "Событие ScheduleChanged должно было вызваться");
    }
}
