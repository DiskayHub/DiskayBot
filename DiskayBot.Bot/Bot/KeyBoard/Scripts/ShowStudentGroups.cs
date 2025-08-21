using System.Data;
using DiskayBot.API.Services;
using DiskayBot.Bot.Events.Data;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DiskayBot.Bot.Bot.KeyBoard.Scripts;

public class ShowStudentGroups {
    public readonly string Name;
    private readonly UserService _userService;
    public ShowStudentGroups(string name, UserService userService) {
        Name = name;
        _userService = userService;
    }
    public async Task<UserKeyboard?> GetKeyboard(short course, string nextCallBack) {
        var courseGroups = await _userService.GetCourseGroups(course);
        
        courseGroups = courseGroups.OrderBy(c => {
            var parts = c.name.Split('-');
            return int.Parse(parts[1]);
        }).ToList();
        
        var userButtons = courseGroups.Select(group =>
             new UserButton(group.name, $"{nextCallBack}={group.id}")
        ).ToList();
        
        var userKeyboard = new UserKeyboard(Name, userButtons);
        return userKeyboard;
    }
}