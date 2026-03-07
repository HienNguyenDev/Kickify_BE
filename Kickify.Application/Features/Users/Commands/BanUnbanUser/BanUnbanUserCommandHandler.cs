using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Users.Commands.BanUnbanUser;

public class BanUnbanUserCommandHandler : ICommandHandler<BanUnbanUserCommand, BanUnbanUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BanUnbanUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BanUnbanUserResponse>> Handle(
        BanUnbanUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
        {
            return Result.Failure<BanUnbanUserResponse>(UserErrors.NotFound(request.UserId));
        }

        if (!request.IsActive)
        {
            // Ban: isActive must be true currently
            if (!user.IsActive)
            {
                return Result.Failure<BanUnbanUserResponse>(UserErrors.AlreadyBanned);
            }
            user.IsActive = false;
        }
        else
        {
            // Unban: isActive must be false currently
            if (user.IsActive)
            {
                return Result.Failure<BanUnbanUserResponse>(UserErrors.NotBanned);
            }
            user.IsActive = true;
            user.BannedUntil = null; // Clear ban duration khi unban
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var message = !request.IsActive ? "User has been banned" : "User has been unbanned";
        return Result.Success(new BanUnbanUserResponse(user.UserId, user.IsActive, message));
    }
}
