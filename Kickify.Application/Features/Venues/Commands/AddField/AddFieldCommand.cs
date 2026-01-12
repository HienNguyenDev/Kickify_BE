using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Venues.Commands.AddField
{
    public record AddFieldCommand(
        Guid VenueId,
        string Name,
        string FieldType,
        int MaxPlayers,
        decimal PricePerHour,
        string? Description
    ) : IRequest<Result<AddFieldResponse>>;
}
