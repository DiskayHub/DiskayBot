using DiskayBot.API.Contracts;

namespace DiskayBot.Services.ScheduleService.Data;

public record GroupScheduleAnalyse(
    TimeOnly StartTime,
    TimeOnly EndTime,
    int ItemsCount,
    TimeSpan StudyDuration,
    string? LlmAnalyse
);
