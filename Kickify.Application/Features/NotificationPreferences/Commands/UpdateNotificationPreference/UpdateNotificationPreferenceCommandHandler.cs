using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.NotificationPreferences.Commands.UpdateNotificationPreference;

public class UpdateNotificationPreferenceCommandHandler : ICommandHandler<UpdateNotificationPreferenceCommand, UpdateNotificationPreferenceCommandResponse>
{
    private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public UpdateNotificationPreferenceCommandHandler(
        INotificationPreferenceRepository notificationPreferenceRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _notificationPreferenceRepository = notificationPreferenceRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<UpdateNotificationPreferenceCommandResponse>> Handle(UpdateNotificationPreferenceCommand request, CancellationToken cancellationToken)
    {
        var preference = await _notificationPreferenceRepository.GetByUserIdAsync(_userContext.UserId, cancellationToken);

        if (preference is null)
            return Result.Failure<UpdateNotificationPreferenceCommandResponse>(NotificationPreferenceErrors.NotFound);

        preference.MatchRoom = request.MatchRoom;
        preference.Friendship = request.Friendship;
        preference.Post = request.Post;
        if (request.Chat.HasValue)
        {
            preference.Chat = request.Chat.Value;
        }

        _notificationPreferenceRepository.Update(preference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new UpdateNotificationPreferenceCommandResponse
        {
            PreferenceId = preference.PreferenceId,
            UserId = preference.UserId,
            MatchRoom = preference.MatchRoom,
            Friendship = preference.Friendship,
            Post = preference.Post,
            Chat = preference.Chat,
            UpdatedAt = preference.UpdatedAt
        };

        return Result.Success(response);
    }
}
