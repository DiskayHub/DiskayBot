using System;

namespace DiskayBot.Infrastructure.Contracts.Groups;

public record GroupResponse(
    Guid id,
    string name,
    int course
);
