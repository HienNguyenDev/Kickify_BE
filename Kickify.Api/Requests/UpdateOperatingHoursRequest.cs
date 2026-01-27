namespace Kickify.Api.Requests;

public class UpdateOperatingHoursRequest
{
    public List<OperatingHourItemRequest> OperatingHours { get; set; } = new();
}

public record OperatingHourItemRequest(
    int DayOfWeek,
    string? OpenTime,
    string? CloseTime
);
