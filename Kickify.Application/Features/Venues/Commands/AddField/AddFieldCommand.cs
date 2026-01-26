using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Commands.AddField
{
    public record AddFieldCommand(
        Guid VenueId,
        string Name,
        string FieldType,
        int MaxPlayers,
        decimal PricePerHour,
        string? Description
    ) : ICommand<AddFieldResponse>;
}
