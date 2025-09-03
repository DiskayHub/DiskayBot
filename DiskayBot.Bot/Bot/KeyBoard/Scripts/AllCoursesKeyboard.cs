using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.KeyBoard.Scripts;

public class AllCoursesKeyboard {
    
    public static InlineKeyboardMarkup GetKeyboard(string callBack) {
        List<string> allCourses = ["1", "2", "3", "4"];

        var buttons = allCourses.Select(course => {
            return InlineKeyboardButton.WithCallbackData(course, $"{callBack}={course}");
        }).ToList();
        
        return new InlineKeyboardMarkup(buttons);
    }
}