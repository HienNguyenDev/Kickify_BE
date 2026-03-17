namespace Kickify.Api.Requests
{
    public record AddFieldRequest
    {
        public string Name { get; init; } = string.Empty;
        public string FieldType { get; init; } = string.Empty;
        public string? SurfaceType { get; init; }
        public decimal HourlyRate { get; init; }
        public decimal PeakHourSurcharge { get; init; }
        public TimeSpan? PeakStartTime { get; init; }
        public TimeSpan? PeakEndTime { get; init; }
        public decimal WeekendSurcharge { get; init; }
        public decimal HolidaySurcharge { get; init; }
        public List<Kickify.Domain.Enums.DayOfWeekEnum>? PeakDaysOfWeek { get; init; }
        public bool? IsPeakHourSurchargePercentage { get; init; }
        public bool? IsWeekendSurchargePercentage { get; init; }
        public bool? IsHolidaySurchargePercentage { get; init; }
    }
}
