using DiskayBot.API.Clients;
using DiskayBot.API.Exeptions;
using DiskayBot.API.Interfaces;
using Moq;
using Moq.Protected;
using Xunit.Abstractions;

namespace DiskayBot.Tests;

using Xunit;
using Moq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;

public class ScheduleClientTests
{
    [Fact]
    public async Task GetActualScheduleWeek_ShouldReturnWeekSchedule_WhenServerReturnsData() {
        // Arrange
        var groupName = "ИТ25-11";
        var fakeApiResponse = """
                              [
                              {
                                  "ClID": "1199",
                                  "type": "2",
                                  "Day": "2025-09-29",
                                  "group": "ИТ24-11",
                                  "topic": "",
                                  "start": "2025-09-29 10:45",
                                  "end": "2025-09-29 12:15",
                                  "room": "",
                                  "color": "blueviolet",
                                  "title": "ПрофПредмет",
                                  "SubGroup": [
                                      {
                                          "SClID": "1204",
                                          "SGrID": "FE",
                                          "SGCaID": "2-3",
                                          "STopic": "",
                                          "STitle": "ИнстРазрИнтерф"
                                      }
                                  ]
                              },
                              {
                                  "ClID": "1200",
                                  "type": "2",
                                  "Day": "2025-09-29",
                                  "group": "ИТ24-11",
                                  "topic": "",
                                  "start": "2025-09-29 13:00",
                                  "end": "2025-09-29 14:30",
                                  "room": "",
                                  "color": "blueviolet",
                                  "title": "АнглЯзПро",
                                  "SubGroup": [
                                      {
                                          "SClID": "1205",
                                          "SGrID": "A1.22",
                                          "SGCaID": "405",
                                          "STopic": "",
                                          "STitle": "АнглЯзПро"
                                      },
                                      {
                                          "SClID": "1206",
                                          "SGrID": "A1.21",
                                          "SGCaID": "403",
                                          "STopic": "",
                                          "STitle": "АнглЯзПро"
                                      },
                                      {
                                          "SClID": "1207",
                                          "SGrID": "A2.21",
                                          "SGCaID": "410",
                                          "STopic": "",
                                          "STitle": "АнглЯзПро"
                                      },
                                      {
                                          "SClID": "1208",
                                          "SGrID": "B1.21",
                                          "SGCaID": "404",
                                          "STopic": "",
                                          "STitle": "АнглЯзПро"
                                      }
                                  ]
                              },
                              {
                                  "ClID": "1201",
                                  "Day": "2025-09-29",
                                  "group": "ИТ24-11",
                                  "topic": "",
                                  "start": "2025-09-29 14:45",
                                  "end": "2025-09-29 16:15",
                                  "room": "2-4",
                                  "color": "blueviolet",
                                  "title": "ЭлВышМат"
                              }
                              ]
                              """;

        // Создаём мок HttpMessageHandler
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fakeApiResponse)
            });

        // Создаём HttpClient с мокнутым handler
        var httpClient = new HttpClient(handlerMock.Object);

        // Создаём ScheduleClient
        var client = new ScheduleClient(httpClient, "https://portal.it-college.ru", "College");

        // Act
        var result = await client.GetActualScheduleWeek(groupName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(groupName, result.Schedule[0].mainGroup); // проверяем, что ключ соответствует группе
    }

    [Fact]
    public async Task GetActualScheduleWeek_ShouldThrowException_WhenServerReturnsError() {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var client = new ScheduleClient(httpClient, "https://portal.it-college.ru", "College");

        // Act & Assert
        await Assert.ThrowsAsync<ConnectionRefuseExeption>(async () =>
        {
            await client.GetActualScheduleWeek("ИТ25-11");
        });
    }
}
