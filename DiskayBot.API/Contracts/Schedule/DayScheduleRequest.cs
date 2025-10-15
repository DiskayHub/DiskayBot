using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DiskayBot.API.Modules;
using DiskayBot.API.Validators;

namespace DiskayBot.API.Contracts.Schedule;

public class DayScheduleRequest {
    public string d_start { get; init; }
    public string d_end { get; init; }
    public string group { get; init; }
    public string subgroup { get; init; }
    
    private DayScheduleRequest(string dayStart, string dayEnd, string group, string subgroup) {
        d_start = dayStart;
        d_end = dayEnd;
        this.group = group;
        this.subgroup = subgroup;
    }

    public StringContent GetStringContent() {
        var jsonOptions = new JsonSerializerOptions {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = null,
        };
        
        var jsonData = JsonSerializer.Serialize(this, jsonOptions);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        return content;
    }
    public static DayScheduleRequest? Create(string dayStart, string dayEnd, string group, string subgroup = "*") {
        var dayScheduleRequest = new DayScheduleRequest(dayStart, dayEnd, group, subgroup);
        var result = new DayScheduleRequestValidator().Validate(dayScheduleRequest);
        if (result.IsValid) {
            return dayScheduleRequest;
        }
        return null;
    }

    public static DayScheduleRequest? Create(TimePeriod period, string group, string subgroup = "*") {
        var dayScheduleRequest = new DayScheduleRequest(
            dayStart: period.Start.ToString("yyyy-MM-dd"),
            dayEnd: period.End.ToString("yyyy-MM-dd"),
            group: group,
            subgroup: subgroup
        );
        var result = new DayScheduleRequestValidator().Validate(dayScheduleRequest);
        if (result.IsValid) {
            return dayScheduleRequest;
        }
        return null;
    }
}