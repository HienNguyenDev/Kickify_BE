using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Friendships.Commands.RemoveFriend;

public class RemoveFriendCommandHandler : ICommandHandler<RemoveFriendCommand, RemoveFriendCommandResponse>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public RemoveFriendCommandHandler(IFriendshipRepository friendshipRepository, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _friendshipRepository = friendshipRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<RemoveFriendCommandResponse>> Handle(RemoveFriendCommand request, CancellationToken cancellationToken)
    {
        var friendship = await _friendshipRepository.GetFriendshipAsync(_userContext.UserId, request.FriendId, cancellationToken);
        if (friendship is null) return Result.Failure<RemoveFriendCommandResponse>(FriendshipErrors.NotFriends);

        _friendshipRepository.Remove(friendship);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RemoveFriendCommandResponse
        {
            FriendId = request.FriendId,
            RemovedAt = DateTime.UtcNow
        };

        return Result.Success(response);
    }
}
