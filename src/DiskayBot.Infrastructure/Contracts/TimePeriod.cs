namespace DiskayBot.Infrastructure.Contracts;

public record TimePeriod(
    DateOnly Start,
    DateOnly End
);