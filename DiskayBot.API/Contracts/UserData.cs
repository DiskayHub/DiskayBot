namespace DiskayBot.API.Contracts;

public record UserData(
    string username, 
    string group_name,
    string? sub_group,
    string? eng_group,
    string? prof_group
);