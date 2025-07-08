using System;

namespace DiskayBot.API.Contracts.Students.CreateUser;

public record CreateUserRequest(
    string Name,
    string Password
);
