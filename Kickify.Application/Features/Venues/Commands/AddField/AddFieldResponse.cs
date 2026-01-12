namespace Kickify.Application.Features.Venues.Commands.AddField
{
    public record AddFieldResponse(
        Guid FieldId,
        Guid VenueId,
        string Name,
        string FieldType,
        int MaxPlayers,
        decimal PricePerHour,
        string? Description,
        DateTime CreatedAt
    );
}
