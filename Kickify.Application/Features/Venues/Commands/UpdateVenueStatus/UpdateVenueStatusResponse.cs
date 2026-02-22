namespace Kickify.Application.Features.Venues.Commands.UpdateVenueStatus;

public record UpdateVenueStatusResponse(
    Guid VenueId,
    string VenueName,
    string PreviousStatus,
    string CurrentStatus,
    string? AdminNotes,
    DateTime UpdatedAt
);
