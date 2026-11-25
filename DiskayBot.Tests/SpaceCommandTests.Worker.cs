using System.Net;
using DiskayBot.API.Clients;
using DiskayBot.API.Clients.Options;
using DiskayBot.API.Contracts;
using DiskayBot.API.Contracts.Schedule;
using DiskayBot.API.Interfaces;
using DiskayBot.API.Modules;
using DiskayBot.Bot.ScheduleService;
using DiskayBot.Bot.ScheduleService.Options;
using DiskayBot.Bot.ScheduleService.Worker;
using DiskayBot.Redis.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace DiskayBot.Tests;

public partial class SpaceCommandTests {
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Worker_publishes_only_complete_current_week_data(bool currentWeekFails, bool nextWeekFails) {
        const string first = "ИТ26-11";
        const string second = "ИТ26-12";
        var today = DateOnly.FromDateTime(DateTime.Now);
        var currentWeek = TimeHelper.GetWeekPeriod(today);
        var todaySchedule = new DaySchedule(today, first, [new DayItem("Математика", "", "403", new(11, 0), new(12, 0), null)]);
        var otherDay = today == currentWeek.Start ? today.AddDays(1) : today.AddDays(-1);
        var state = new RoomScheduleState();
        var previous = new RoomScheduleSnapshot(today, []);
        state.Publish(previous);
        var nextWeekStarted = new TaskCompletionSource<RoomScheduleSnapshot?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var schedule = new Mock<IScheduleClient>();
        schedule.Setup(client => client.GetWeekSchedule(It.IsAny<string>(), It.IsAny<TimePeriod>()))
            .Returns((string group, TimePeriod week) => {
                if (week.Start != currentWeek.Start) {
                    nextWeekStarted.TrySetResult(state.Read());
                    if (nextWeekFails) {
                        throw new HttpRequestException("Next week request failed");
                    }
                    return Task.FromResult<GroupWeekSchedule?>(null);
                }
                if (group == second && currentWeekFails) {
                    throw new HttpRequestException("Current week request failed");
                }
                return Task.FromResult<GroupWeekSchedule?>(group == first
                    ? new GroupWeekSchedule(week, [todaySchedule, new DaySchedule(otherDay, first, [])])
                    : null);
            });
        using var http = new HttpClient(new TestHttpHandler((request, _) => {
            Assert.Equal("/api/groups", request.RequestUri!.AbsolutePath);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("""
                    [{"id":"00000000-0000-0000-0000-000000000001","name":"ИТ26-11","course":1},
                     {"id":"00000000-0000-0000-0000-000000000002","name":"ИТ26-12","course":1}]
                    """)
            });
        }));
        var users = new UserClient(http, Options.Create(new UserClientOptions {
            url = "https://memory.example", name = "DiskayMemory"
        }), NullLogger<UserClient>.Instance);
        using var worker = new ScheduleBackgroundService(Mock.Of<IMediator>(), schedule.Object, Mock.Of<IRedisController>(),
            users, state, Options.Create(new ScheduleServiceOptions { updateTimeout = 3600 }),
            NullLogger<ScheduleBackgroundService>.Instance);
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        RoomScheduleSnapshot? published;
        await worker.StartAsync(deadline.Token);
        try {
            published = await nextWeekStarted.Task.WaitAsync(deadline.Token);
        }
        finally {
            await worker.StopAsync(deadline.Token);
        }

        if (currentWeekFails) {
            Assert.Null(published);
            Assert.Null(state.Read());
        }
        else {
            Assert.NotNull(published);
            Assert.Equal(today, published.Date);
            Assert.Equal(todaySchedule, Assert.Single(published.Days));
            Assert.Same(published, state.Read());
        }
    }

    private sealed class TestHttpHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
        : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            send(request, cancellationToken);
    }
}
