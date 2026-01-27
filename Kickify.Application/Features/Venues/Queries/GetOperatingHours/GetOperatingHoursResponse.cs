namespace Kickify.Application.Features.Venues.Queries.GetOperatingHours;

public class GetOperatingHoursResponse
{
    public Guid VenueId { get; set; }
    public List<OperatingHourDto> OperatingHours { get; set; } = new();
}

public record OperatingHourDto(
    Guid HoursId,
    int DayOfWeek,
    string DayName,
    TimeSpan? OpenTime,
    TimeSpan? CloseTime,
    bool IsClosed
);
