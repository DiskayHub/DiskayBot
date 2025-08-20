namespace DiskayBot.Bot.Bot.KeyBoard;

public class KeyboardHandler {
    private readonly Dictionary<string, UserKeyboard> _keyBoards;

    public KeyboardHandler() {
        _keyBoards = new Dictionary<string, UserKeyboard>();
    }
    
    public KeyboardHandler(List<UserKeyboard> userKeyboards) {
        _keyBoards = userKeyboards.ToDictionary(userKeyboard => userKeyboard.Name);
    }

    public void BindKeyboard(UserKeyboard userKeyboard) {
        if (!_keyBoards.ContainsKey(userKeyboard.Name)){
            _keyBoards.Add(userKeyboard.Name, userKeyboard);
        }
        else{
            throw new Exception($"KEYBOARD_BIND_ERROR: Имя {userKeyboard.Name} уже занято");
        }
    }

    public UserKeyboard? GetKeyBoard(string name) {
        return _keyBoards.TryGetValue(name, out var userKeyboard) ? userKeyboard : null;
    }
}