using System.Net;
using System.Text;
using System.Text.Json;
using DiskayBot.Infrastructure.Clients.Options;
using DiskayBot.Infrastructure.Contracts;
using DiskayBot.Infrastructure.Contracts.Groups;
using DiskayBot.Infrastructure.Contracts.Service;
using DiskayBot.Infrastructure.Contracts.Users.GetUser;
using DiskayBot.Infrastructure.Contracts.Users.UpdateUser;
using DiskayBot.Infrastructure.Exeptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiskayBot.Infrastructure.Clients;

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
        try{
            _logger.LogInformation("Проверка статуса сервиса DiskayMemory");
            _logger.LogDebug($"Отправка запроса по url: {_options.url}/api/service/ping");
            
            var response = await _client.GetAsync($"{_options.url}/api/service/ping");

            if (response.IsSuccessStatusCode){
                _logger.LogInformation("Проверка статуса сервисов завершена");
                var content = await response.Content.ReadAsStringAsync();
                var responseStatus = JsonSerializer.Deserialize<PingResponse>(content);
                return responseStatus;
            }
            
            throw new HttpRequestException();
        }
        catch (HttpRequestException){
            _logger.LogCritical("Обращение к сервису DiskayMemory завершилось с ошибкой!");
            _logger.LogDebug("Не удлалось сделать подключится к сервису DiskayMemory, отправляю статус с проблемой");
            return PingResponse.CreateDefault(_options.name);
        }
    }

    public async Task<HttpStatusCode> Registration(long userId, string userName, string groupId) {
        string jsonContent = JsonSerializer.Serialize(new {
            user_id = userId,
            username = userName,
            group_id = groupId
        });
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{_options.url}/api/diskay/telegram_user", content);
        
        Console.WriteLine(response.RequestMessage);
        if (response.IsSuccessStatusCode) {
            return HttpStatusCode.OK;   
        }
        return HttpStatusCode.InternalServerError;
    }

    public async Task<UserData?> Authorization(long userId) {
        _logger.LogInformation($"Авторизация через сервис DiskayMemory по id: {userId}");
        _logger.LogDebug("Сериализация тела запроса");
        string jsonContent = JsonSerializer.Serialize(new {
            user_id = userId
        });
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        _logger.LogDebug("Отправка запрос на сервер");
        HttpResponseMessage response = await _client.GetAsync($"{_options.url}/api/telegram_users/{userId}");

        if (response.IsSuccessStatusCode) {
            _logger.LogInformation($"Пользователь прошёл авторизацию");
            var responseBody = await response.Content.ReadAsStringAsync();
            var userData = JsonSerializer.Deserialize<UserData>(responseBody);
            _logger.LogDebug($"Авторизовался пользователь '{userData?.username}', Группа = {userData.group_name}");
            return userData;
        }

        if (response.StatusCode == HttpStatusCode.NotFound){
            _logger.LogInformation($"Пользователь с id ({userId}) не прошёл авторизацию");
            return null;
        }
        
        _logger.LogCritical("Ошибка при авторизации!");
        throw new ConnectionRefuseExeption(_options.name);
    }

    public async Task<List<GroupResponse>?> GetAllGroups() {
        _logger.LogInformation("Получаю все существующие группы");
        
        _logger.LogDebug($"Отправка запроса по url: {_options.url}/api/groups");
        HttpResponseMessage response = await _client.GetAsync($"{_options.url}/api/groups");
        
        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Ответ от DiskayMemory получен");
            _logger.LogDebug("Дессериализация строки от сервера");
            string responseBody = await response.Content.ReadAsStringAsync();
            var groups = JsonSerializer.Deserialize<List<GroupResponse>>(responseBody);

            if (groups != null) {
                _logger.LogDebug($"Группы возвращены. Всего групп: {groups.Count}");
                return groups;
            }
            _logger.LogDebug("Групп нет, отправляю null");
            return null;
        }
        
        _logger.LogCritical("Ошибка при получении групп!");
        throw new ConnectionRefuseExeption(_options.name);
    }

    public async Task<List<GroupResponse>?> GetCourseGroups(int course) {
        _logger.LogInformation($"Получение групп по курсу: {course}");
        _logger.LogDebug($"Отправка запроса по url: {_options.url}/api/groups/{course}");
        
        HttpResponseMessage response =
            await _client.GetAsync($"{_options.url}/api/groups/{course}");
        if (response.IsSuccessStatusCode){
            _logger.LogInformation("Ответ от DiskayMemory получен");
            _logger.LogDebug("Дессериализация строки от сервера");
            
            string responseBody = await response.Content.ReadAsStringAsync();
            var groups = JsonSerializer.Deserialize<List<GroupResponse>>(responseBody);
            
            _logger.LogDebug($"Группы возвращены. Всего групп: {groups.Count}");
            return groups;
        }

        _logger.LogCritical($"Ошибка при получении групп по номеру курса!");
        throw new ConnectionRefuseExeption(_options.name);
    }

    public async Task<HttpStatusCode> UpdateUser(long userId, UpdateUserRequest requestBody) {
        _logger.LogInformation($"Обновление данных пользователя с id: {userId}");
        
        var jsonString = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
        
        _logger.LogDebug($"Отправка запроса по url: {_options.url}/api/telegram_users/{userId}");
        var response = await _client.PutAsync($"{_options.url}/api/telegram_users/{userId}", content);

        if (response.IsSuccessStatusCode) {
            _logger.LogInformation($"Пользователь обновлён! Новая группа пользователя: {requestBody.group_id}");
            return HttpStatusCode.OK;
        }
        if (response.StatusCode == HttpStatusCode.InternalServerError) {
            return HttpStatusCode.InternalServerError; 
        }
        if (response.StatusCode == HttpStatusCode.NotFound) {
            return HttpStatusCode.NotFound;
        }
        
        _logger.LogCritical("Ошибка при обновлении пользователя!");
        throw new ConnectionRefuseExeption(_options.name);
    }

    public async Task<List<TelegramUser>?> GetAllUsers() {
        _logger.LogInformation("Получаю список всех пользователей");
        var response = await _client.GetAsync($"{_options.url}/api/telegram_users");
        if (response.IsSuccessStatusCode) {
            var stringContent = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<TelegramUser>>(stringContent);
                _logger.LogDebug($"Пользователи получены, количество пользователей: {users.Count}");
            return users;
        }
        _logger.LogCritical("Не удалось получить всех пользователей!");
        throw new ConnectionRefuseExeption(_options.name);
    }

    public async Task<List<TelegramUser>?> GetNotifyUsers() {
        _logger.LogInformation("Получаю список пользователей, подписанных на уведомления");
        var response = await _client.GetAsync($"{_options.url}/api/telegram_users/notify");
        if (response.IsSuccessStatusCode) {
            var stringContent = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<TelegramUser>>(stringContent);
            _logger.LogDebug($"Пользователи получены, количество: {users?.Count}");
            return users;
        }
        _logger.LogCritical("Не удалось получить пользователей для уведомлений!");
        throw new ConnectionRefuseExeption(_options.name);
    }
}
