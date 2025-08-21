using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.KeyBoard;

public class UserKeyboard {
    public readonly string Name;
    private readonly Dictionary<string, UserButton> _buttons;
    
    public UserKeyboard(string name, List<UserButton> buttons) {
        Name = name;
        _buttons = buttons.ToDictionary(button => button.Text);
    }

    public InlineKeyboardMarkup GetInlineKeyboard() {
        List<InlineKeyboardButton> inlineButtons = _buttons.Keys.Select(buttonName => {
            var buttonCallback = _buttons[buttonName].CallBack;
            return InlineKeyboardButton.WithCallbackData(buttonName, $"{Name}:{buttonCallback}");
        }).ToList();
        return new InlineKeyboardMarkup(inlineButtons);
    }
}