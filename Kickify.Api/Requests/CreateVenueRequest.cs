namespace Kickify.Api.Requests
{
    public record CreateVenueRequest
    {
        public Guid OwnerId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public decimal Latitude { get; init; }
        public decimal Longitude { get; init; }
        public string? Description { get; init; }
        public List<CreateVenueFieldRequest> Fields { get; init; } = new();
        public List<CreateVenueOperatingHoursRequest> OperatingHours { get; init; } = new();
    }

    public record CreateVenueFieldRequest
    {
        public string Name { get; init; } = string.Empty;
        public string FieldType { get; init; } = string.Empty;
        public int MaxPlayers { get; init; }
        public decimal PricePerHour { get; init; }
        public string? Description { get; init; }
    }

    public record CreateVenueOperatingHoursRequest
    {
        public DayOfWeek DayOfWeek { get; init; }
        public TimeSpan OpenTime { get; init; }
        public TimeSpan CloseTime { get; init; }
    }
}
