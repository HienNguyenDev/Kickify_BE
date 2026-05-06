using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Kickify.Domain.Event;
using MediatR;

namespace Kickify.Application.Features.MatchRooms.Commands.InviteFriendToRoom;

public class InviteFriendToRoomCommandHandler : ICommandHandler<InviteFriendToRoomCommand, InviteFriendToRoomResponse>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IRoomInvitationRepository _roomInvitationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IPublisher _publisher;

    public InviteFriendToRoomCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IRoomInvitationRepository roomInvitationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IPublisher publisher)
    {
        _matchRoomRepository = matchRoomRepository;
        _roomInvitationRepository = roomInvitationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _publisher = publisher;
    }

    public async Task<Result<InviteFriendToRoomResponse>> Handle(InviteFriendToRoomCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        // Cannot invite yourself
        if (userId == request.FriendUserId)
            return Result.Failure<InviteFriendToRoomResponse>(MatchRoomErrors.CannotInviteSelf);

        // Check room exists and is Open
        var room = await _matchRoomRepository.GetRoomWithParticipantsAsync(request.RoomId, cancellationToken);
        if (room is null)
            return Result.Failure<InviteFriendToRoomResponse>(MatchRoomErrors.NotFound(request.RoomId));

        if (room.Status != RoomStatus.Open)
            return Result.Failure<InviteFriendToRoomResponse>(MatchRoomErrors.NotOpen);

        // Verify current user is a participant
        var isParticipant = room.RoomParticipants.Any(p => p.UserId == userId);
        if (!isParticipant)
            return Result.Failure<InviteFriendToRoomResponse>(MatchRoomErrors.NotParticipant);

        // Check if friend is already in the room
        var friendAlreadyInRoom = room.RoomParticipants.Any(p => p.UserId == request.FriendUserId);
        if (friendAlreadyInRoom)
            return Result.Failure<InviteFriendToRoomResponse>(MatchRoomErrors.AlreadyJoined);

        // Check room is not full
        if (room.FilledSlots >= room.TotalSlots)
            return Result.Failure<InviteFriendToRoomResponse>(MatchRoomErrors.RoomFull);

        // Check if there's already a pending invitation
        var existingInvitation = await _roomInvitationRepository.GetPendingInvitationAsync(request.RoomId, request.FriendUserId, cancellationToken);
        if (existingInvitation is not null)
            return Result.Failure<InviteFriendToRoomResponse>(MatchRoomErrors.InvitationAlreadySent);

        // Get inviter info for event
        var inviter = await _userRepository.GetByIdAsync(userId);
        var inviterName = inviter?.FullName ?? "Một người bạn";

        var deepLink = $"kickify://room/{room.RoomId}";
        var webLink = $"https://api.kickify.site/invite/room/{room.RoomId}";

        // Create invitation entity
        var invitation = new RoomInvitation
        {
            InvitationId = Guid.NewGuid(),
            RoomId = room.RoomId,
            InviterId = userId,
            InviteeId = request.FriendUserId,
            InvitationLink = webLink,
            QrCodeUrl = deepLink,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _roomInvitationRepository.AddAsync(invitation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event for notification + push notification
        await _publisher.Publish(new RoomInvitationSentDomainEvent(
            invitation.InvitationId,
            room.RoomId,
            userId,
            request.FriendUserId,
            inviterName,
            room.RoomName), cancellationToken);

        return Result.Success(new InviteFriendToRoomResponse(
            invitation.InvitationId,
            room.RoomId,
            userId,
            request.FriendUserId,
            deepLink,
            invitation.Status.ToString(),
            invitation.CreatedAt
        ));
    }
}
