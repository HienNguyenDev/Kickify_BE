using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class VenueOperatingHour
{
    public Guid HoursId { get; set; }
    public Guid VenueId { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public bool IsClosed { get; set; } = false;

    // Navigation properties
    public Venue Venue { get; set; } = null!;
}
