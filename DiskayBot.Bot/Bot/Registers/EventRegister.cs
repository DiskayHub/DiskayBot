using DiskayBot.Bot.Abstractions;
using DiskayBot.Bot.Interfaces;

namespace DiskayBot.Bot.Bot.Registers;

public class EventRegister {
    private Dictionary<string, EventProcessor> _events;

    public EventRegister() {
        _events = new Dictionary<string, EventProcessor>();
    }

    public void HandleEvent(string key, EventProcessor evt) {
        _events[key] = evt;
    }

    public EventProcessor? GetEvent(string key) {
        _events.TryGetValue(key, out var evt);
        return evt;
    }
}