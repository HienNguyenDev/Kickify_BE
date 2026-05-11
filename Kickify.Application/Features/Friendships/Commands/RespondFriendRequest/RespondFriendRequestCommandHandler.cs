using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Kickify.Domain.Event;
using MediatR;

namespace Kickify.Application.Features.Friendships.Commands.RespondFriendRequest;

public class RespondFriendRequestCommandHandler : ICommandHandler<RespondFriendRequestCommand, RespondFriendRequestCommandResponse>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IPublisher _publisher;

    public RespondFriendRequestCommandHandler(
        IFriendshipRepository friendshipRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IPublisher publisher)
    {
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _publisher = publisher;
    }

    public async Task<Result<RespondFriendRequestCommandResponse>> Handle(RespondFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendship = await _friendshipRepository.GetFriendshipAsync(request.RequesterId, _userContext.UserId, cancellationToken);
        if (friendship is null || friendship.Status != FriendshipStatus.Pending) return Result.Failure<RespondFriendRequestCommandResponse>(FriendshipErrors.RequestNotFound);
        if (friendship.AddresseeId != _userContext.UserId) return Result.Failure<RespondFriendRequestCommandResponse>(FriendshipErrors.Unauthorized);

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

        if (request.Accept)
        {
            var addressee = await _userRepository.GetByIdAsync(_userContext.UserId);
            var addresseeName = addressee?.FullName ?? addressee?.Email ?? "Người dùng";

            await _publisher.Publish(new FriendRequestAcceptedDomainEvent(
                friendship.FriendshipId,
                friendship.RequesterId,
                friendship.AddresseeId,
                addresseeName), cancellationToken);
        }

        var response = new RespondFriendRequestCommandResponse
        {
            FriendshipId = friendship.FriendshipId,
            RequesterId = friendship.RequesterId,
            Status = request.Accept ? FriendshipStatus.Accepted.ToString() : "Declined",
            RespondedAt = DateTime.UtcNow
        };

        return Result.Success(response);
    }
}
