using Kickify.Application.Abstractions.Jobs;
using Kickify.Domain.Event;
using MediatR;

namespace Kickify.Application.Features.MatchRooms.Events;

public class MatchRoomCreatedEventHandler : INotificationHandler<MatchRoomCreatedDomainEvent>
{
    private readonly IRoomAutoCloseService _roomAutoCloseService;
    private static readonly TimeSpan AutoCloseDelay = TimeSpan.FromMinutes(20);

    public MatchRoomCreatedEventHandler(IRoomAutoCloseService roomAutoCloseService)
    {
        _roomAutoCloseService = roomAutoCloseService;
    }

    public Task Handle(MatchRoomCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _roomAutoCloseService.ScheduleAutoClose(notification.RoomId, AutoCloseDelay);
        return Task.CompletedTask;
    }
}
