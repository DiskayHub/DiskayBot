using DiskayBot.Bot.Bot.KeyBoard;

namespace DiskayBot.Bot.Abstractions;

public abstract class AbstractKeyboard {
    public readonly string Name;

    public AbstractKeyboard(string name) {
        Name = name;
    }

    public abstract UserKeyboard GetKeyboard();
}