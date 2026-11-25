using System.Net;
using System.Text.Json;
using DiskayBot.API.Contracts;
using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Bot.Commands;
using DiskayBot.Bot.Bot.Commands.Base;
using DiskayBot.Bot.Bot.Controllers;
using DiskayBot.Bot.Bot.Registers;
using DiskayBot.Bot.DTOs;
using DiskayBot.Bot.Events;
using DiskayBot.Bot.ScheduleService;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiskayBot.Tests;

public partial class SpaceCommandTests {
    private const string Header = "📅 <b>25.11.2026</b> | 🚪 <b>Свободные кабинеты</b>\n" +
        "<code>——————————————————————————</code>\n";
    private static readonly DateOnly Today = new(2026, 11, 25);
    private static readonly string[] RoomNames = [
        "1-3", "2-1", "2-3", "2-4", "2-5", "3-0", "3-1", "3-2", "3-3", "3-4", "3-5", "3-e",
        "403", "404", "405", "406", "410"
    ];

    [Fact]
    public async Task Dispatch_orders_rooms_and_shows_only_groups_at_the_next_start() {
        var snapshot = new RoomScheduleSnapshot(Today, [
            new DaySchedule(Today, "ИТ26-11", [
                Lesson("404", new(11, 30), new(12, 10)),
                Lesson("410", new(10, 30), new(11, 30)),
                Lesson("403", new(15, 0), new(16, 0))
            ]),
            new DaySchedule(Today, "ИТ26-13", [
                Lesson(null, new(14, 0), new(15, 0), "403", "403")
            ]),
            new DaySchedule(Today, "ИТ26-12", [
                Lesson(null, new(11, 0), new(12, 0), "3-1/2"),
                Lesson("403", new(14, 0), new(15, 0)),
                Lesson("403", new(14, 0), new(15, 0), "403"),
                Lesson("405", new(12, 0), new(13, 0))
            ]),
            new DaySchedule(Today, "ИТ<&>26", [
                Lesson("406", new(11, 30, 45), new(12, 30))
            ])
        ]);

        var text = await Dispatch(snapshot);

        string[] occupiedOrScheduled = ["404", "3-1", "3-2", "403", "405", "406"];
        var allDay = string.Join(" · ", RoomNames.Except(occupiedOrScheduled).Select(name => $"<b>{name}</b>"));
        Assert.Equal(Header + allDay + "\n\n" +
            "<b>До следующей пары</b>\n" +
            "🫡 <b>ИТ26-12, ИТ26-13</b>\n<b>403</b> · до <b>14:00</b>\n\n" +
            "🫡 <b>ИТ26-12</b>\n<b>405</b> · до <b>12:00</b>\n\n" +
            "🫡 <b>ИТ&lt;&amp;&gt;26</b>\n<b>406</b> · до <b>11:30</b>", text);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Dispatch_reports_unavailable_for_missing_or_yesterdays_snapshot(bool yesterday) {
        var snapshot = yesterday ? new RoomScheduleSnapshot(Today.AddDays(-1), []) : null;

        Assert.Equal(Header + "Расписание всех групп на сегодня пока недоступно. Попробуйте позже.",
            await Dispatch(snapshot));
    }

    [Fact]
    public async Task Dispatch_lists_every_room_when_there_are_no_classes() {
        var text = await Dispatch(new RoomScheduleSnapshot(Today, []));

        Assert.Equal(Header + string.Join(" · ", RoomNames.Select(name => $"<b>{name}</b>")), text);
    }

    [Fact]
    public async Task Dispatch_reports_no_free_rooms_when_all_are_busy() {
        var lessons = RoomNames.Where(name => name is not ("1-3" or "3-1" or "3-2"))
            .Select(name => Lesson(name, new(11, 0), new(12, 0))).ToList();
        lessons.Add(Lesson("каб 1-3", new(11, 0), new(12, 0), "3-1/2"));
        var snapshot = new RoomScheduleSnapshot(Today, [new DaySchedule(Today, "ИТ26-11", lessons)]);

        Assert.Equal(Header + "По расписанию сейчас нет свободных кабинетов.", await Dispatch(snapshot));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Refresh_edits_the_original_message_and_handles_unchanged_results(bool unchanged) {
        var latest = new RoomScheduleSnapshot(Today, []);
        var updatedText = await Dispatch(unchanged ? latest : null, latest, unchanged);

        Assert.Equal(Header + string.Join(" · ", RoomNames.Select(name => $"<b>{name}</b>")), updatedText);
    }

    private static async Task<string> Dispatch(RoomScheduleSnapshot? snapshot, RoomScheduleSnapshot? refreshTo = null,
        bool unchanged = false) {
        const long chatId = 123;
        const int messageId = 1;
        const string callbackId = "space-refresh";
        var methods = new List<string>();
        string? callbackData = null;
        var sent = new List<string>();
        using var http = new HttpClient(new TestHttpHandler(async (request, token) => {
            var method = request.RequestUri!.Segments[^1];
            methods.Add(method);
            var body = await request.Content!.ReadAsStringAsync(token);
            using var json = JsonDocument.Parse(body);
            var payload = json.RootElement;
            if (method == "answerCallbackQuery") {
                Assert.Equal(callbackId, payload.GetProperty("callback_query_id").GetString());
                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent("""{"ok":true,"result":true}""")
                };
            }
            Assert.Equal(chatId, payload.GetProperty("chat_id").GetInt64());
            Assert.Equal("HTML", payload.GetProperty("parse_mode").GetString(), ignoreCase: true);
            var row = Assert.Single(payload.GetProperty("reply_markup").GetProperty("inline_keyboard").EnumerateArray());
            var button = Assert.Single(row.EnumerateArray());
            Assert.Equal("Обновить 💫", button.GetProperty("text").GetString());
            var data = button.GetProperty("callback_data").GetString();
            if (method == "editMessageText") {
                Assert.Equal(messageId, payload.GetProperty("message_id").GetInt32());
                Assert.Equal(callbackData, data);
            }
            else {
                callbackData = data;
            }
            sent.Add(payload.GetProperty("text").GetString()!);
            if (method == "editMessageText" && unchanged) {
                return new HttpResponseMessage(HttpStatusCode.BadRequest) {
                    Content = new StringContent("""
                        {"ok":false,"error_code":400,"description":"Bad Request: message is not modified"}
                        """)
                };
            }
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("""
                    {"ok":true,"result":{"message_id":1,"chat":{"id":123,"type":"private"},"date":1795606200,"text":"sent"}}
                    """)
            };
        }));
        var state = new RoomScheduleState();
        state.Publish(snapshot);
        var descriptors = CommandScanner.Scan(typeof(SpaceCommand).Assembly).ToList();
        var services = new ServiceCollection();
        foreach (var type in descriptors.Select(descriptor => descriptor.CommandType).Distinct()) {
            services.AddTransient(type);
        }
        services.AddOptions();
        services.AddSingleton(state);
        services.AddSingleton<TimeProvider>(new FixedClock());
        services.AddSingleton(new CommandRegistry(descriptors));
        services.AddSingleton<CommandDispatcher>();
        using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<CommandDispatcher>();
        var chat = new Chat { Id = chatId, Type = ChatType.Private };
        var ctx = new BotContext {
            Bot = new TelegramBotClient("123456789:abcdefghijklmnopqrstuvwxyz123456789", http),
            Event = new MessageUserEvent(new UpdateInfo(UpdateType.Message, "student",
                chat, chatId, new MessageId { Id = messageId }), "/space"),
            User = new UserData("student", "ИТ26-11", null, null, null, false)
        };

        await dispatcher.DispatchAsync(ctx, CancellationToken.None);
        if (refreshTo == null) {
            Assert.Equal("sendMessage", Assert.Single(methods));
            return Assert.Single(sent);
        }

        state.Publish(refreshTo);
        Assert.False(string.IsNullOrEmpty(callbackData));
        await dispatcher.DispatchAsync(new BotContext {
            Bot = ctx.Bot,
            Event = new CallbackQueryUserEvent(new UpdateInfo(UpdateType.CallbackQuery, "student",
                chat, chatId, new MessageId { Id = messageId }), callbackData, callbackId),
            User = ctx.User
        }, CancellationToken.None);

        Assert.Equal(new[] { "sendMessage", "answerCallbackQuery", "editMessageText" }, methods);
        Assert.Equal(2, sent.Count);
        Assert.Equal(unchanged, sent[0] == sent[1]);
        return sent[1];
    }

    private sealed class FixedClock : TimeProvider {
        public override TimeZoneInfo LocalTimeZone { get; } =
            TimeZoneInfo.CreateCustomTimeZone("UTC+5", TimeSpan.FromHours(5), "UTC+5", "UTC+5");

        public override DateTimeOffset GetUtcNow() => new(Today, new TimeOnly(6, 30), TimeSpan.Zero);
    }
}
