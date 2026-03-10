using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Commands.ToggleVenueArchived;

public record ToggleVenueArchivedCommand(
    Guid VenueId
) : ICommand<ToggleVenueArchivedResponse>;
