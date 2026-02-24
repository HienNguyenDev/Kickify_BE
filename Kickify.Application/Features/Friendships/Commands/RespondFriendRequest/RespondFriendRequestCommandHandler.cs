using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Friendships.Commands.RespondFriendRequest;

public class RespondFriendRequestCommandHandler : ICommandHandler<RespondFriendRequestCommand, RespondFriendRequestCommandResponse>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public RespondFriendRequestCommandHandler(IFriendshipRepository friendshipRepository, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _friendshipRepository = friendshipRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<RespondFriendRequestCommandResponse>> Handle(RespondFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendship = await _friendshipRepository.GetFriendshipAsync(request.RequesterId, _userContext.UserId, cancellationToken);
        if (friendship is null || friendship.Status != FriendshipStatus.Pending) return Result.Failure<RespondFriendRequestCommandResponse>(FriendshipErrors.RequestNotFound);
        if (friendship.AddresseeId != _userContext.UserId) return Result.Failure<RespondFriendRequestCommandResponse>(FriendshipErrors.Unauthorized);

        var friendshipId = friendship.FriendshipId;
        var requesterId = friendship.RequesterId;

        if (request.Accept)
        {
            friendship.Status = FriendshipStatus.Accepted;
            friendship.RespondedAt = DateTime.UtcNow;
            _friendshipRepository.Update(friendship);
        }
        else
        {
            _friendshipRepository.Remove(friendship);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RespondFriendRequestCommandResponse
        {
            FriendshipId = friendshipId,
            RequesterId = requesterId,
            Status = request.Accept ? FriendshipStatus.Accepted.ToString() : "Declined",
            RespondedAt = DateTime.UtcNow
        };

        return Result.Success(response);
    }
}
