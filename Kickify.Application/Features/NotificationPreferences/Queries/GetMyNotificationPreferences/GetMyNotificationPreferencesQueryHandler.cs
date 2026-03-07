using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.NotificationPreferences.Queries.GetMyNotificationPreferences;

public class GetMyNotificationPreferencesQueryHandler : IQueryHandler<GetMyNotificationPreferencesQuery, GetMyNotificationPreferencesResponse>
{
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly IUserContext _userContext;

    public GetMyNotificationPreferencesQueryHandler(
        INotificationPreferenceRepository preferenceRepository,
        IUserContext userContext)
    {
        _preferenceRepository = preferenceRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetMyNotificationPreferencesResponse>> Handle(
        GetMyNotificationPreferencesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        var preference = await _preferenceRepository.GetByUserIdAsync(userId, cancellationToken);
        if (preference == null)
            return Result.Failure<GetMyNotificationPreferencesResponse>(NotificationErrors.PreferenceNotFound);

        return Result.Success(new GetMyNotificationPreferencesResponse(
            preference.PreferenceId,
            preference.MatchRoom,
            preference.Friendship,
            preference.Post));
    }
}
