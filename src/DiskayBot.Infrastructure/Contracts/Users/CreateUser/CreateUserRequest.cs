using System;

namespace DiskayBot.Infrastructure.Contracts.Students.CreateUser;

public record CreateUserRequest (
    long user_id,
    string username,
    Guid group_id,
    string? sub_group,
    string? eng_group,
    string? prof_group
);
