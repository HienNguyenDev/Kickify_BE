using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class Field : BaseEntity
{
    public Guid FieldId { get; set; }
    public Guid VenueId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public string? SurfaceType { get; set; } // Grass, Artificial, etc.
    public decimal HourlyRate { get; set; }
    public decimal WeekendSurcharge { get; set; } = 0;
    public decimal HolidaySurcharge { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsWeekendSurchargePercentage { get; set; } = false;
    public bool IsHolidaySurchargePercentage { get; set; } = false;

    // Navigation properties
    public Venue Venue { get; set; } = null!;
    public ICollection<FieldPeakHour> PeakHours { get; set; } = new List<FieldPeakHour>();
    public ICollection<MatchRoom> MatchRooms { get; set; } = new List<MatchRoom>();
    public ICollection<MatchPreset> MatchPresets { get; set; } = new List<MatchPreset>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
