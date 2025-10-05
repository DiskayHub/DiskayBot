using DiskayBot.API.Clients;
using DiskayBot.API.Contracts;
using DiskayBot.Services.ScheduleService;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ScheduleServiceTests {
    private readonly Mock<ScheduleClient> _mockClient;
    private readonly Mock<ILogger<ScheduleService>> _mockLogger;
    private readonly ScheduleService _service;
    
    public ScheduleServiceTests() {
        _mockClient = new Mock<ScheduleClient>();
        _mockLogger = new Mock<ILogger<ScheduleService>>();
        _service = new ScheduleService(_mockClient.Object, _mockLogger.Object);
    }
    
    [Fact]
    public void Method_GetActualSchedule_returns_actual_schedule() {

    }
}