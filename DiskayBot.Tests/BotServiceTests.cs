using System.Net;
using DiskayBot.API.Services;
using Moq;
using Moq.Protected;

namespace DiskayBot.Tests;

public class BotServiceTests {
    [Fact]
    public async Task RegistrationReturnsOkWhenResponseIsSuccess() {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK
            });
        
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new BotService(httpClient);

        var result = await service.Registration(100, "user", "123");
        
        Assert.Equal(HttpStatusCode.OK, result);
    }
}