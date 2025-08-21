namespace DiskayBot.Bot.Bot.KeyBoard;

public class UserButton {
    public readonly string Text;
    public readonly string CallBack;

    public UserButton(string text, string callBack) {
        Text = text;
        CallBack = callBack;
    }
}