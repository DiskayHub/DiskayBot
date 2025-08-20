using DiskayBot.Bot.Abstractions;

namespace DiskayBot.Bot.Bot.Registers;

public class EventRegister {
    private readonly Dictionary<string, EventProcessor>  _eventHandlers;

    public EventRegister(List<EventProcessor> eventHandlers) {
        _eventHandlers = eventHandlers.ToDictionary(evt => evt.Name);
    }

    public EventProcessor? GetEvent(string eventName) {
        return _eventHandlers.TryGetValue(eventName, out var evt) ? evt : null;
    }
}