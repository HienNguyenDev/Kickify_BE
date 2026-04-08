using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Kickify.Application.Abstractions.Persistence;

namespace Kickify.Application.Features.MatchRooms.Commands.RequestTransferHost;

internal sealed class RequestTransferHostCommandHandler : ICommandHandler<RequestTransferHostCommand, RequestTransferHostResponse>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMatchRoomHubService _matchRoomHubService;

    public RequestTransferHostCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IUserRepository userRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IMatchRoomHubService matchRoomHubService)
    {
        _matchRoomRepository = matchRoomRepository;
        _userRepository = userRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _matchRoomHubService = matchRoomHubService;
    }

    public async Task<Result<RequestTransferHostResponse>> Handle(RequestTransferHostCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;

        var room = await _matchRoomRepository.GetRoomWithParticipantsForUpdateAsync(request.RoomId, cancellationToken);
        if (room is null)
        {
            return Result.Failure<RequestTransferHostResponse>(MatchRoomErrors.NotFound(request.RoomId));
        }

        if (room.HostId != currentUserId)
        {
            return Result.Failure<RequestTransferHostResponse>(MatchRoomErrors.CurrentUserNotHost);
        }

        if (room.Status != RoomStatus.Open)
        {
            return Result.Failure<RequestTransferHostResponse>(MatchRoomErrors.RoomNotOpen);
        }

        if (room.HostId == request.TargetUserId)
        {
            return Result.Failure<RequestTransferHostResponse>(MatchRoomErrors.TargetUserIsAlreadyHost);
        }

        var targetParticipant = room.RoomParticipants.FirstOrDefault(p => p.UserId == request.TargetUserId);
        if (targetParticipant is null)
        {
            return Result.Failure<RequestTransferHostResponse>(MatchRoomErrors.TargetUserNotParticipant);
        }

        if (room.PendingHostTransferId.HasValue)
        {
            var pendingUserStillInRoom = room.RoomParticipants.Any(p => p.UserId == room.PendingHostTransferId.Value);

            if (!pendingUserStillInRoom)
            {
                room.PendingHostTransferId = null;
            }
            else if (room.PendingHostTransferId.Value != request.TargetUserId)
            {
                return Result.Failure<RequestTransferHostResponse>(MatchRoomErrors.HostTransferRequestAlreadyPending);
            }
            else
            {
                // Idempotent behavior for repeated requests to the same target.
                var host = await _userRepository.GetByIdAsync(currentUserId);

                await _matchRoomHubService.NotifyHostTransferRequestedAsync(
                    room.RoomId,
                    request.TargetUserId,
                    host?.FullName ?? host?.Email ?? "The Host",
                    cancellationToken);

                return Result.Success(new RequestTransferHostResponse(true));
            }
        }

        room.PendingHostTransferId = request.TargetUserId;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var hostUser = await _userRepository.GetByIdAsync(currentUserId);

        await _matchRoomHubService.NotifyHostTransferRequestedAsync(
            room.RoomId,
            request.TargetUserId,
            hostUser?.FullName ?? hostUser?.Email ?? "The Host",
            cancellationToken);

        return Result.Success(new RequestTransferHostResponse(true));
    }
}
