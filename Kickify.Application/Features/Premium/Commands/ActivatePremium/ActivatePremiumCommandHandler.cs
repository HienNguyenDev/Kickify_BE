using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Common;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Premium.Commands.ActivatePremium;

public class ActivatePremiumCommandHandler : ICommandHandler<ActivatePremiumCommand, ActivatePremiumResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivatePremiumCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ActivatePremiumResponse>> Handle(
        ActivatePremiumCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
            return Result.Failure<ActivatePremiumResponse>(UserErrors.NotFound(request.UserId));

        // If already premium and not yet expired, extend from current expiry; otherwise start fresh from now
        var baseDate = user.IsPremium && user.PremiumExpireAt.HasValue && user.PremiumExpireAt > DateTime.UtcNow
            ? user.PremiumExpireAt.Value
            : DateTime.UtcNow;

        user.IsPremium = true;
        user.PremiumExpireAt = baseDate.Add(PlatformConstants.PremiumDuration);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ActivatePremiumResponse(user.UserId, user.IsPremium, user.PremiumExpireAt.Value));
    }
}
