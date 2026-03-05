using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.KeyBoard.Scripts;

public class GlobalKeyboard {
    
    public static InlineKeyboardMarkup GetCoursesKeyboard(string callBack) {
        List<string> allCourses = ["1", "2", "3", "4"];

        var buttons = allCourses.Select(course => {
            return InlineKeyboardButton.WithCallbackData(course, $"{callBack}={course}");
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