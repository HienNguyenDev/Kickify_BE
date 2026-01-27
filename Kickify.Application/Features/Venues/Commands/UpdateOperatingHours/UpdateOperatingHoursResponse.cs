namespace Kickify.Application.Features.Venues.Commands.UpdateOperatingHours;

public class UpdateOperatingHoursResponse
{
    public Guid VenueId { get; set; }
    public List<OperatingHourResultDto> OperatingHours { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}

public record OperatingHourResultDto(
    Guid HoursId,
    int DayOfWeek,
    string DayName,
    TimeSpan? OpenTime,
    TimeSpan? CloseTime,
    bool IsClosed
);
