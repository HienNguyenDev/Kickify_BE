using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Commands.ToggleVenueSuspension;

public record ToggleVenueSuspensionCommand(
    Guid VenueId
) : ICommand<ToggleVenueSuspensionResponse>;
