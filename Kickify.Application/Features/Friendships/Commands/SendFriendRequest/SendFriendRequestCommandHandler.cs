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

namespace Kickify.Application.Features.Friendships.Commands.SendFriendRequest;

public class SendFriendRequestCommandHandler : ICommandHandler<SendFriendRequestCommand, SendFriendRequestCommandResponse>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IPublisher _publisher;

    public SendFriendRequestCommandHandler(
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

    public async Task<Result<SendFriendRequestCommandResponse>> Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        if (request.AddresseeId == _userContext.UserId) return Result.Failure<SendFriendRequestCommandResponse>(FriendshipErrors.CannotAddSelf);

        var addressee = await _userRepository.GetByIdAsync(request.AddresseeId);
        if (addressee is null) return Result.Failure<SendFriendRequestCommandResponse>(FriendshipErrors.UserNotFound);

        var requester = await _userRepository.GetByIdAsync(_userContext.UserId);
        if (requester is null) return Result.Failure<SendFriendRequestCommandResponse>(FriendshipErrors.UserNotFound);

        var existingFriendship = await _friendshipRepository.GetFriendshipIncludeDeletedAsync(_userContext.UserId, request.AddresseeId, cancellationToken);
        
        Friendship friendship;

        if (existingFriendship is not null)
        {
            if (existingFriendship.DeletedAt is not null)
            {
                existingFriendship.DeletedAt = null;
                existingFriendship.RequesterId = _userContext.UserId;
                existingFriendship.AddresseeId = request.AddresseeId;
                existingFriendship.Status = FriendshipStatus.Pending;
                existingFriendship.RespondedAt = null;
                _friendshipRepository.Update(existingFriendship);
                friendship = existingFriendship;
            }
            else
            {
                if (existingFriendship.Status == FriendshipStatus.Accepted) return Result.Failure<SendFriendRequestCommandResponse>(FriendshipErrors.AlreadyFriends);
                if (existingFriendship.Status == FriendshipStatus.Pending) return Result.Failure<SendFriendRequestCommandResponse>(FriendshipErrors.RequestAlreadyExists);
                return Result.Failure<SendFriendRequestCommandResponse>(FriendshipErrors.RequestAlreadyExists);
            }
        }
        else
        {
            friendship = new Friendship
            {
                FriendshipId = Guid.NewGuid(),
                RequesterId = _userContext.UserId,
                AddresseeId = request.AddresseeId,
                Status = FriendshipStatus.Pending
            };
            await _friendshipRepository.AddAsync(friendship);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event tr?c ti?p sau khi l?u
        await _publisher.Publish(new FriendRequestSentDomainEvent(
            friendship.FriendshipId,
            _userContext.UserId,
            request.AddresseeId,
            requester.FullName ?? requester.Email), cancellationToken);

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
