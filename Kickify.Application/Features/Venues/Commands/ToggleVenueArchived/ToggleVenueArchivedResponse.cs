namespace Kickify.Application.Features.Venues.Commands.ToggleVenueArchived;

public record ToggleVenueArchivedResponse(
    Guid VenueId,
    string VenueName,
    string PreviousStatus,
    string CurrentStatus,
    DateTime UpdatedAt
);
