using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Friendships.Commands.SendFriendRequest;

public class SendFriendRequestCommandHandler : ICommandHandler<SendFriendRequestCommand, SendFriendRequestCommandResponse>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public SendFriendRequestCommandHandler(IFriendshipRepository friendshipRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<SendFriendRequestCommandResponse>> Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        if (request.AddresseeId == _userContext.UserId) return Result.Failure<SendFriendRequestCommandResponse>(FriendshipErrors.CannotAddSelf);

        var addressee = await _userRepository.GetByIdAsync(request.AddresseeId);
        if (addressee is null) return Result.Failure<SendFriendRequestCommandResponse>(FriendshipErrors.UserNotFound);

        var existingFriendship = await _friendshipRepository.GetFriendshipAsync(_userContext.UserId, request.AddresseeId, cancellationToken);
        if (existingFriendship is not null)
        {
            if (existingFriendship.Status == FriendshipStatus.Accepted) return Result.Failure<SendFriendRequestCommandResponse>(FriendshipErrors.AlreadyFriends);
            if (existingFriendship.Status == FriendshipStatus.Pending) return Result.Failure<SendFriendRequestCommandResponse>(FriendshipErrors.RequestAlreadyExists);
        }

        var friendship = new Friendship
        {
            FriendshipId = Guid.NewGuid(),
            RequesterId = _userContext.UserId,
            AddresseeId = request.AddresseeId,
            Status = FriendshipStatus.Pending
        };

        await _friendshipRepository.AddAsync(friendship);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new SendFriendRequestCommandResponse
        {
            FriendshipId = friendship.FriendshipId,
            RequesterId = friendship.RequesterId,
            AddresseeId = friendship.AddresseeId,
            Status = friendship.Status.ToString(),
            CreatedAt = friendship.CreatedAt
        };

        return Result.Success(response);
    }
}
