using DiskayBot.Bot.Abstractions;

namespace DiskayBot.Bot.Bot.KeyBoard.Scripts;

public class ShowAllCourses : AbstractKeyboard {
    
    public ShowAllCourses(string name) : base(name) {}

    public UserKeyboard GetReplyMarkup(string nextCallBack) {
        List<string> coursesList = ["1", "2", "3", "4"];

        List<UserButton> userButtons = coursesList.Select(course => 
            new UserButton(course, nextCallBack)
        ).ToList();

        return new UserKeyboard(Name, userButtons);
    }
}