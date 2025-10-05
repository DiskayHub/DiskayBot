using DiskayBot.API.Clients;
using DiskayBot.API.Contracts;
using DiskayBot.API.Interfaces;
using DiskayBot.API.Modules;
using DiskayBot.Services.ScheduleService;
using DiskayBot.Services.ScheduleService.Components;
using DiskayBot.Services.ScheduleService.Data;
using Microsoft.Extensions.Logging;
using Moq;

namespace DiskayBot.Tests.Services;

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

public class ScheduleAnalyserTests {
    private WeekSchedule CreateSchedule(string groupName, string dayName, DateOnly start, DateOnly end) {
        var period = new TimePeriod(start, end);
        var schedule = new WeekSchedule(period, new() {
            [groupName] = new List<DaySchedule>
            {
                new DaySchedule(
                    new DateOnly(2025, 10, 5),
                    groupName,
                    new List<DayItem>
                    {
                        new DayItem(dayName, "Math", "A-101", new TimeOnly(9,0), new TimeOnly(10,30), null)
                    }
                )
            }
        });
        return schedule;
    }

    [Fact]
    public void Listen_ShouldInitializeLastSchedule_OnFirstScheduleAppear() {
        // Arrange
        var loggerMock = new Mock<ILogger<ScheduleAnalyser>>();
        var serviceLoggerMock = new Mock<ILogger<ScheduleService>>();
        var clientMock = new Mock<IScheduleClient>();
        var service = new ScheduleService(clientMock.Object, serviceLoggerMock.Object);
        var analyser = new ScheduleAnalyser(service, loggerMock.Object);

        analyser.Listen();

        // Act
        service.RaiseFirstScheduleAppear();

        // Assert
        Assert.NotNull(service.Schedule);
    }

    [Fact]
    public void Analyse_ShouldInvokeNewWeekScheduleAppear_WhenWeekPeriodChanges() {
        // Arrange
        var loggerMock = new Mock<ILogger<ScheduleAnalyser>>();
        var serviceLoggerMock = new Mock<ILogger<ScheduleService>>();
        var clientMock = new Mock<IScheduleClient>();
        var service = new ScheduleService(clientMock.Object, serviceLoggerMock.Object);
        var analyser = new ScheduleAnalyser(service, loggerMock.Object);

        analyser.Listen();

        bool newWeekTriggered = false;
        analyser.NewWeekScheduleAppear += () => newWeekTriggered = true;

        var oldSchedule = CreateSchedule("ИТ25-11", "Понедельник",
            new DateOnly(2025, 10, 5), new DateOnly(2025, 10, 11));
        var newSchedule = CreateSchedule("ИТ25-11", "Вторник",
            new DateOnly(2025, 10, 12), new DateOnly(2025, 10, 18));

        // Инициализируем "_lastSchedule"
        service.RaiseFirstScheduleAppear();
        typeof(ScheduleAnalyser)
            .GetField("_lastSchedule", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(analyser, oldSchedule);

        // Act
        service.RaiseScheduleUpdated(newSchedule);

        // Assert
        Assert.True(newWeekTriggered);
    }

    [Fact]
    public void Analyse_ShouldNotInvokeEvent_WhenWeekPeriodSame() {
        // Arrange
        var loggerMock = new Mock<ILogger<ScheduleAnalyser>>();
        var serviceLoggerMock = new Mock<ILogger<ScheduleService>>();
        var clientMock = new Mock<IScheduleClient>();
        var service = new ScheduleService(clientMock.Object, serviceLoggerMock.Object);
        var analyser = new ScheduleAnalyser(service, loggerMock.Object);

        analyser.Listen();

        bool newWeekTriggered = false;
        analyser.NewWeekScheduleAppear += () => newWeekTriggered = false;

        var schedule1 = CreateSchedule("ИТ25-11", "Понедельник",
            new DateOnly(2025, 10, 5), new DateOnly(2025, 10, 11));
        var schedule2 = CreateSchedule("ИТ25-11", "Понедельник",
            new DateOnly(2025, 10, 5), new DateOnly(2025, 10, 11));

        service.RaiseFirstScheduleAppear();
        typeof(ScheduleAnalyser)
            .GetField("_lastSchedule", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(analyser, schedule1);

        // Act
        service.RaiseScheduleUpdated(schedule2);

        // Assert
        Assert.False(newWeekTriggered);
    }
}
