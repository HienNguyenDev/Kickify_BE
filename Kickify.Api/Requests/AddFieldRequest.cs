namespace Kickify.Api.Requests
{
    public record AddFieldRequest
    {
        public string Name { get; init; } = string.Empty;
        public string FieldType { get; init; } = string.Empty;
        public string? SurfaceType { get; init; }
        public decimal HourlyRate { get; init; }
        public decimal WeekendSurcharge { get; init; }
        public decimal HolidaySurcharge { get; init; }
        public List<FieldPeakHourDto>? PeakHours { get; init; }
        public bool? IsWeekendSurchargePercentage { get; init; }
        public bool? IsHolidaySurchargePercentage { get; init; }
    }
}
