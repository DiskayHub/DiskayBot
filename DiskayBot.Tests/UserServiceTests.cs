using System.Net;
using System.Text;
using DiskayBot.API.Contracts;
using DiskayBot.API.Services;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DiskayBot.Tests;

public class UserServiceTests {
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
        var service = new UserService(httpClient);

        var result = await service.Registration(100, "user", "123");
        
        Assert.Equal(HttpStatusCode.OK, result);
    }
    
    [Fact]
    public async Task RegistrationReturnsIntenalErrorWhenResponseIsUnSuccess() {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.InternalServerError
            });
        
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new UserService(httpClient);
        
        var result = await service.Registration(100, "user", "123");
    }

    [Fact]
    public async Task RegistrationReturnsBadRequestWhenRequestIsError() {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Simulated network failure")
            );
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new UserService(httpClient);
        
        var result = await service.Registration(100, "user", "123");
        
        Assert.Equal(HttpStatusCode.BadRequest, result);
    }

    [Fact]
    public async Task AuthorizationReturnsUserDataWhenRequestIsSuccess() {
        var handlerMock = new Mock<HttpMessageHandler>();

        var expectedUser = new UserData("Ivan", "24-13");
        
        var userJson = JsonSerializer.Serialize(expectedUser);
        var content = new StringContent(userJson, Encoding.UTF8, "application/json");

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = content
            });
        
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new UserService(httpClient);
        
        var result = await service.Authorization(123);
        
        Assert.NotNull(result);
        Assert.Equal(expectedUser.username, result.username);
        Assert.Equal(expectedUser.group_name, result.group_name);
    }
    
    [Fact]
    public async Task AuthorizationReturnsNULLWhenRequestIsNotFound() {
        var handlerMock = new Mock<HttpMessageHandler>();
        
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.NotFound
            });
        
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new UserService(httpClient);
        
        var result = await service.Authorization(123);
        
        Assert.Null(result);
    }
}