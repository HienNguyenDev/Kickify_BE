using Kickify.Application.Abstractions.Jobs;
using Kickify.Domain.Event;
using MediatR;

namespace Kickify.Application.Features.MatchRooms.Events;

public class ParticipantJoinedRoomEventHandler : INotificationHandler<ParticipantJoinedRoomDomainEvent>
{
    private readonly IRoomAutoCloseService _roomAutoCloseService;
    private static readonly TimeSpan AutoCloseDelay = TimeSpan.FromMinutes(20);

    public ParticipantJoinedRoomEventHandler(IRoomAutoCloseService roomAutoCloseService)
    {
        _roomAutoCloseService = roomAutoCloseService;
    }

    public Task Handle(ParticipantJoinedRoomDomainEvent notification, CancellationToken cancellationToken)
    {
        _roomAutoCloseService.RescheduleAutoClose(notification.RoomId, notification.OldAutoCloseJobId, AutoCloseDelay);
        return Task.CompletedTask;
    }
}
