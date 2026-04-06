using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.MatchRooms.Commands.RespondTransferHost;

internal sealed class RespondTransferHostCommandHandler : ICommandHandler<RespondTransferHostCommand, RespondTransferHostResponse>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMatchRoomHubService _matchRoomHubService;

    public RespondTransferHostCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IUserRepository userRepository,
        IChatMessageRepository chatMessageRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IMatchRoomHubService matchRoomHubService)
    {
        _matchRoomRepository = matchRoomRepository;
        _userRepository = userRepository;
        _chatMessageRepository = chatMessageRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _matchRoomHubService = matchRoomHubService;
    }

    public async Task<Result<RespondTransferHostResponse>> Handle(RespondTransferHostCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;

        var room = await _matchRoomRepository.GetRoomWithParticipantsForUpdateAsync(request.RoomId, cancellationToken);
        if (room is null)
        {
            return Result.Failure<RespondTransferHostResponse>(MatchRoomErrors.NotFound(request.RoomId));
        }

        if (room.Status != RoomStatus.Open)
        {
            return Result.Failure<RespondTransferHostResponse>(MatchRoomErrors.RoomNotOpen);
        }

        if (room.PendingHostTransferId != currentUserId)
        {
            return Result.Failure<RespondTransferHostResponse>(MatchRoomErrors.NotTargetTransferUser);
        }

        var responderParticipant = room.RoomParticipants.FirstOrDefault(p => p.UserId == currentUserId);
        if (responderParticipant is null)
        {
            room.PendingHostTransferId = null;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<RespondTransferHostResponse>(MatchRoomErrors.TargetUserNoLongerParticipant);
        }

        var oldHostId = room.HostId;

        if (!request.IsAccepted)
        {
            room.PendingHostTransferId = null;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var currentUser = await _userRepository.GetByIdAsync(currentUserId);
            
            await _matchRoomHubService.NotifyHostTransferRejectedAsync(
                room.RoomId,
                oldHostId,
                currentUser?.FullName ?? currentUser?.Email ?? "A participant",
                cancellationToken);

            return Result.Success(new RespondTransferHostResponse(true));
        }

        room.HostId = currentUserId;
        room.PendingHostTransferId = null;

        var chatMessage = new ChatMessage
        {
            MessageId = Guid.NewGuid(),
            SenderId = currentUserId,
            RoomId = room.RoomId,
            MessageText = "The host role has been transferred to a new player.",
            MessageType = MessageType.System,
            ConversationType = ConversationType.Room,
            SentAt = DateTime.UtcNow
        };

        await _chatMessageRepository.AddAsync(chatMessage);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var newHostUser = await _userRepository.GetByIdAsync(currentUserId);

        await _matchRoomHubService.NotifyHostTransferredAsync(
            room.RoomId,
            currentUserId,
            newHostUser?.FullName ?? newHostUser?.Email ?? "New Host",
            cancellationToken);

        return Result.Success(new RespondTransferHostResponse(true));
    }
}
