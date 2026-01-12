namespace Kickify.Api.Requests
{
    public record AddFieldRequest
    {
        public string Name { get; init; } = string.Empty;
        public string FieldType { get; init; } = string.Empty;
        public int MaxPlayers { get; init; }
        public decimal PricePerHour { get; init; }
        public string? Description { get; init; }
    }
}
