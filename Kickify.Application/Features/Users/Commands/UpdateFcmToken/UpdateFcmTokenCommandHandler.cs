using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Users.Commands.UpdateFcmToken;

public class UpdateFcmTokenCommandHandler : ICommandHandler<UpdateFcmTokenCommand, UpdateFcmTokenCommandResponse>
{
    private const string GlobalAnnouncementTopic = "all_users";

    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPushNotificationService _pushNotificationService;

    public UpdateFcmTokenCommandHandler(
        IUserRepository userRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IPushNotificationService pushNotificationService)
    {
        _userRepository = userRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _pushNotificationService = pushNotificationService;
    }

    public async Task<Result<UpdateFcmTokenCommandResponse>> Handle(UpdateFcmTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_userContext.UserId);
        if (user == null)
        {
            return Result.Failure<UpdateFcmTokenCommandResponse>(UserErrors.NotFound(_userContext.UserId));
        }

        // Unsubscribe old token from topic before replacing
        if (!string.IsNullOrEmpty(user.FcmToken) && user.FcmToken != request.FcmToken)
        {
            await _pushNotificationService.UnsubscribeFromTopicAsync(user.FcmToken, GlobalAnnouncementTopic, cancellationToken);
        }

        user.FcmToken = request.FcmToken;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Subscribe new token to global announcement topic
        if (!string.IsNullOrEmpty(request.FcmToken))
        {
            await _pushNotificationService.SubscribeToTopicAsync(request.FcmToken, GlobalAnnouncementTopic, cancellationToken);
        }

        return Result.Success(new UpdateFcmTokenCommandResponse { Success = true });
    }
}

