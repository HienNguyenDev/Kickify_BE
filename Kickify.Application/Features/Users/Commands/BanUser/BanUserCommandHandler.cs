using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Users.Commands.BanUser;

public class BanUserCommandHandler : ICommandHandler<BanUserCommand, BanUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BanUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BanUserResponse>> Handle(BanUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return Result.Failure<BanUserResponse>(UserErrors.NotFound(request.UserId));

        if (user.Role == UserRole.Admin)
            return Result.Failure<BanUserResponse>(BanErrors.CannotBanAdmin);

        var now = DateTime.UtcNow;
        DateTime? bannedUntil;
        string durationLabel;

        if (request.Duration == BanDuration.Permanent)
        {
            bannedUntil = null;
            durationLabel = "Permanent";
        }
        else
        {
            bannedUntil = now.AddDays((int)request.Duration);
            durationLabel = $"{(int)request.Duration} day(s)";
        }

        user.IsActive = false;
        user.BannedUntil = bannedUntil;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var message = request.Duration == BanDuration.Permanent
            ? $"User '{user.Email}' has been permanently banned"
            : $"User '{user.Email}' has been banned for {durationLabel} until {bannedUntil:yyyy-MM-dd HH:mm} UTC";

        return Result.Success(new BanUserResponse(
            user.UserId,
            user.Email,
            user.IsActive,
            user.BannedUntil,
            durationLabel,
            message));
    }
}
