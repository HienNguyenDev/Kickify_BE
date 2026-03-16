namespace Kickify.Api.Requests
{
    public record CreateVenueRequest
    {
        // OwnerId is extracted from JWT token, not from request body
        public string Name { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public decimal Latitude { get; init; }
        public decimal Longitude { get; init; }
        public string? ContactPhone { get; init; }
        public string? ContactEmail { get; init; }
        public string? Description { get; init; }
        public string? Amenities { get; init; }
        public List<Guid> IgnoredHolidayIds { get; init; } = new();
        public List<CreateVenueFieldRequest> Fields { get; init; } = new();
        public List<CreateVenueOperatingHoursRequest> OperatingHours { get; init; } = new();
    }

    public record CreateVenueFieldRequest
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
     }

    public record CreateVenueOperatingHoursRequest
    {
        public int DayOfWeek { get; init; }
        public TimeSpan OpenTime { get; init; }
        public TimeSpan CloseTime { get; init; }
    }
}
