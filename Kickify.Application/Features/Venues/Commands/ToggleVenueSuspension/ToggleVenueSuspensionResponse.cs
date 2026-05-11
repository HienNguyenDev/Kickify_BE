namespace Kickify.Application.Features.Venues.Commands.ToggleVenueSuspension;

public record ToggleVenueSuspensionResponse(
    Guid VenueId,
    string VenueName,
    string PreviousStatus,
    string CurrentStatus,
    DateTime UpdatedAt
);
