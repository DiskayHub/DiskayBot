namespace DiskayBot.Bot.Bot.KeyBoard;

public class UserButton {
    public readonly string Name;
    public readonly string CallBack;
    private readonly Func<Task> _command;

    public UserButton(string name, string callBack, Func<Task> command) {
        Name = name;
        CallBack = callBack;
        _command = command;
    }

    public async Task Press() {
        await _command();
    }
}