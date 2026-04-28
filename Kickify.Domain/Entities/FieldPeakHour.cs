using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class FieldPeakHour
{
    public Guid Id { get; set; }
    public Guid FieldId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal SurchargeAmount { get; set; }
    public bool IsPercentage { get; set; }
    public List<DayOfWeekEnum> ApplicableDays { get; set; } = new();

    public Field Field { get; set; } = null!;
}
