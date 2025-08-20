namespace DiskayBot.Bot.Bot.KeyBoard;

public class UserButton {
    public readonly string Name;
    public readonly string CallBack;

    public UserButton(string name, string callBack) {
        Name = name;
        CallBack = callBack;
    }
}