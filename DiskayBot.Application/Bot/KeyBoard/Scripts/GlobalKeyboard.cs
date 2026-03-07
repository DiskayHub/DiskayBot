using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.KeyBoard.Scripts;

public class GlobalKeyboard {

    public static InlineKeyboardMarkup GetSettingsKeyboard(bool notify) {
        var notifyLabel = notify ? "🔔 Уведомления: вкл" : "🔕 Уведомления: выкл";
        return new InlineKeyboardMarkup([
            [InlineKeyboardButton.WithCallbackData("Изменить данные о профиле", "changeProfileData")],
            [InlineKeyboardButton.WithCallbackData(notifyLabel, "toggleNotify")]
        ]);
    }

    public static InlineKeyboardMarkup GetProfileDataKeyboard() {
        return new InlineKeyboardMarkup([
            [InlineKeyboardButton.WithCallbackData("Изменить группу", "changeCourse")],
            [InlineKeyboardButton.WithCallbackData("<-- Вернуться назад", "showSettings")]
        ]);
    }

    public static InlineKeyboardMarkup GetCoursesKeyboard(string callBack) {
        List<string> allCourses = ["1", "2", "3", "4"];

        var buttons = allCourses.Select(course => {
            return InlineKeyboardButton.WithCallbackData($"{course} курс", $"{callBack}={course}");
        }).ToList();
        
        return new InlineKeyboardMarkup(buttons);
    }

    public static InlineKeyboardMarkup GetScheduleNavigatorKeyboard(DateOnly scheduleDate) {
        return new InlineKeyboardMarkup(new[] {
            new[] {
                InlineKeyboardButton.WithCallbackData("←", $"prevSchedule={scheduleDate.ToString("dd.MM.yyyy")}"),
                InlineKeyboardButton.WithCallbackData("→", $"nextSchedule={scheduleDate.ToString("dd.MM.yyyy")}")
            },
            new[] { InlineKeyboardButton.WithCallbackData("Обновить 💫", "updateSchedule") }
        });
    }
}