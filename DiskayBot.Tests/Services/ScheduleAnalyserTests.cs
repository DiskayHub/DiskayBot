using DiskayBot.API.Clients;
using DiskayBot.API.Contracts;
using DiskayBot.API.Interfaces;
using DiskayBot.API.Modules;
using DiskayBot.Services.ScheduleService;
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
    private WeekSchedule CreateSchedule(string groupName, DateOnly start, DateOnly end, string itemName) {
        var period = new TimePeriod(start, end);
        return new WeekSchedule(period, new Dictionary<string, List<DaySchedule>>
        {
            [groupName] = new List<DaySchedule>
            {
                new DaySchedule(
                    new DateOnly(2025, 10, 5),
                    groupName,
                    new List<DayItem>
                    {
                        new DayItem(itemName, "Desc", "A-101", new TimeOnly(9,0), new TimeOnly(10,30), null)
                    }
                )
            }
        });
    }

    [Fact]
    public void Analyse_ShouldRaiseNewWeekScheduleAppear_WhenWeekPeriodChanges()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ScheduleAnalyser>>();
        var serviceLoggerMock = new Mock<ILogger<ScheduleService>>();
        var clientMock = new Mock<IScheduleClient>();

        var service = new ScheduleService(clientMock.Object, serviceLoggerMock.Object);
        var analyser = new ScheduleAnalyser(service, loggerMock.Object);
        analyser.Listen();

        bool eventTriggered = false;
        analyser.NewWeekScheduleAppear += () => eventTriggered = true;

        var oldSchedule = CreateSchedule("ИТ25-11", new DateOnly(2025,10,5), new DateOnly(2025,10,11), "Math");
        var newSchedule = CreateSchedule("ИТ25-11", new DateOnly(2025,10,12), new DateOnly(2025,10,18), "Math");

        // Инициализируем _lastSchedule через событие OnFirstScheduleAppear
        service.Schedule = oldSchedule;
        service.RaiseFirstScheduleAppear();

        // Act
        service.RaiseScheduleUpdated(newSchedule);

        // Assert
        Assert.True(eventTriggered, "Должно быть вызвано событие NewWeekScheduleAppear, когда меняется WeekPeriod");
    }

    [Fact]
    public void Analyse_ShouldLog_WhenGroupScheduleChanges() {
        // Arrange
        var loggerMock = new Mock<ILogger<ScheduleAnalyser>>();
        var serviceLoggerMock = new Mock<ILogger<ScheduleService>>();
        var clientMock = new Mock<IScheduleClient>();

        var service = new ScheduleService(clientMock.Object, serviceLoggerMock.Object);
        var analyser = new ScheduleAnalyser(service, loggerMock.Object);
        analyser.Listen();

        var oldSchedule = CreateSchedule("ИТ25-11", new DateOnly(2025,10,5), new DateOnly(2025,10,11), "Math");
        var newSchedule = CreateSchedule("ИТ25-11", new DateOnly(2025,10,5), new DateOnly(2025,10,11), "Physics"); // поменял предмет

        // Инициализируем _lastSchedule через событие
        service.Schedule = oldSchedule;
        service.RaiseFirstScheduleAppear();

        // Act
        service.RaiseScheduleUpdated(newSchedule);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("РАСПИСАНИЕ ДЛЯ ГРУППЫ ИТ25-11 НЕ СООТВЕТСТВУЕТ НОВОМУ РАСПИСАНИЮ")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }
}
