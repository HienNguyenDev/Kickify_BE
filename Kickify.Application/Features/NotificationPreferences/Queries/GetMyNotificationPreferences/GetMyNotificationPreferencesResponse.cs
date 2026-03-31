namespace Kickify.Application.Features.NotificationPreferences.Queries.GetMyNotificationPreferences;

public record GetMyNotificationPreferencesResponse(
    Guid PreferenceId,
    bool MatchRoom,
    bool Friendship,
    bool Post,
    bool Chat);
