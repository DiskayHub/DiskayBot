namespace DiskayBot.API.Contracts.Users.UpdateUser;

public record UpdateUserRequest(
    Guid? group_id,
    string? sub_group,
    string? eng_group,
    string? prof_group
);