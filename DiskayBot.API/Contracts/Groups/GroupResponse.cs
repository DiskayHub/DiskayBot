using System;

namespace DiskayBot.API.Contracts.Groups;

public record GroupResponse(
    Guid id,
    string name
);
