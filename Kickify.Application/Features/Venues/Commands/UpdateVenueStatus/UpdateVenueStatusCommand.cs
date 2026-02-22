using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Commands.UpdateVenueStatus;

public record UpdateVenueStatusCommand(
    Guid VenueId,
    string Status,
    string? AdminNotes
) : ICommand<UpdateVenueStatusResponse>;
