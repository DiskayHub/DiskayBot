using System.Net;
using System.Text;
using System.Text.Json;
using DiskayBot.API.Clients.Options;
using DiskayBot.API.Contracts;
using DiskayBot.API.Contracts.Groups;
using DiskayBot.API.Contracts.Service;
using DiskayBot.API.Contracts.Users.GetUser;
using DiskayBot.API.Contracts.Users.UpdateUser;
using DiskayBot.API.Exeptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiskayBot.API.Clients;

public class UserClient {
    private readonly HttpClient _client;
    private readonly UserClientOptions _options;
    private readonly ILogger<UserClient> _logger;

    public UserClient(HttpClient client, IOptions<UserClientOptions> options, ILogger<UserClient> logger) {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PingResponse?> PingService() {
        try {
            var response = await _client.GetAsync($"{_options.url}/api/service/ping");
            if (response.IsSuccessStatusCode) {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PingResponse>(content);
            }

            _logger.LogWarning("Проверка статуса {Service}: {Request}", _options.name, DescribeFailure(response));
        }
        catch (HttpRequestException ex) {
            _logger.LogWarning(ex, "Проверка статуса {Service}: сервис недоступен", _options.name);
        }
        return PingResponse.CreateDefault(_options.name);
    }

    public async Task<HttpStatusCode> Registration(long userId, string userName, string groupId) {
        string jsonContent = JsonSerializer.Serialize(new {
            user_id = userId,
            username = userName,
            group_id = groupId
        });
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{_options.url}/api/diskay/telegram_user", content);

        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Пользователь {UserId} зарегистрирован в группе {GroupId}", userId, groupId);
            return HttpStatusCode.OK;
        }

        _logger.LogWarning("Не удалось зарегистрировать пользователя {UserId}: {Request}", userId, DescribeFailure(response));
        return HttpStatusCode.InternalServerError;
    }

    public async Task<UserData?> Authorization(long userId) {
        HttpResponseMessage response = await _client.GetAsync($"{_options.url}/api/telegram_users/{userId}");

        if (response.IsSuccessStatusCode) {
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserData>(responseBody);
        }

        if (response.StatusCode == HttpStatusCode.NotFound) {
            _logger.LogDebug("Пользователь {UserId} не зарегистрирован", userId);
            return null;
        }

        throw RequestFailed(response);
    }

    public async Task<List<GroupResponse>?> GetAllGroups() {
        HttpResponseMessage response = await _client.GetAsync($"{_options.url}/api/groups");

        if (response.IsSuccessStatusCode) {
            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<GroupResponse>>(responseBody);
        }

        throw RequestFailed(response);
    }

    public async Task<List<GroupResponse>?> GetCourseGroups(int course) {
        HttpResponseMessage response = await _client.GetAsync($"{_options.url}/api/groups/{course}");

        if (response.IsSuccessStatusCode) {
            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<GroupResponse>>(responseBody);
        }

        throw RequestFailed(response);
    }

    public async Task<HttpStatusCode> UpdateUser(long userId, UpdateUserRequest requestBody) {
        var jsonString = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{_options.url}/api/telegram_users/{userId}", content);

        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Данные пользователя {UserId} обновлены: {@Changes}", userId, requestBody);
            return HttpStatusCode.OK;
        }
        if (response.StatusCode is HttpStatusCode.InternalServerError or HttpStatusCode.NotFound) {
            _logger.LogWarning("Не удалось обновить пользователя {UserId}: {Request}", userId, DescribeFailure(response));
            return response.StatusCode;
        }

        throw RequestFailed(response);
    }

    public async Task<List<TelegramUser>?> GetAllUsers() {
        var response = await _client.GetAsync($"{_options.url}/api/telegram_users");
        if (response.IsSuccessStatusCode) {
            var stringContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TelegramUser>>(stringContent);
        }

        throw RequestFailed(response);
    }

    public async Task<List<TelegramUser>?> GetNotifyUsers() {
        var response = await _client.GetAsync($"{_options.url}/api/telegram_users/notify");
        if (response.IsSuccessStatusCode) {
            var stringContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TelegramUser>>(stringContent);
        }

        throw RequestFailed(response);
    }

    private ConnectionRefuseExeption RequestFailed(HttpResponseMessage response) {
        return new ConnectionRefuseExeption(_options.name, DescribeFailure(response));
    }

    private static string DescribeFailure(HttpResponseMessage response) {
        var request = response.RequestMessage;
        return $"{request?.Method} {request?.RequestUri} вернул {(int)response.StatusCode} {response.StatusCode}";
    }
}
