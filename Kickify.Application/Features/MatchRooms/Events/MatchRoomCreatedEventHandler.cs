using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Events;

public class MatchRoomCreatedEventHandler : INotificationHandler<MatchRoomCreatedDomainEvent>
{
    private readonly ILogger<MatchRoomCreatedEventHandler> _logger;

    public MatchRoomCreatedEventHandler(ILogger<MatchRoomCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(MatchRoomCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("MatchRoom {RoomId} created.", notification.RoomId);
        // Note: AutoClose is now handled directly by CreateMatchRoomCommandHandler
        return Task.CompletedTask;
    }
}
