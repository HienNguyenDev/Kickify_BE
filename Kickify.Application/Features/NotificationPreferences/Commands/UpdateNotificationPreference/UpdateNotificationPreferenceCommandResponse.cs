namespace Kickify.Application.Features.NotificationPreferences.Commands.UpdateNotificationPreference;

public class UpdateNotificationPreferenceCommandResponse
{
    public Guid PreferenceId { get; set; }
    public Guid UserId { get; set; }
    public bool MatchRoom { get; set; }
    public bool Friendship { get; set; }
    public bool Post { get; set; }
    public bool Chat { get; set; }
    public DateTime UpdatedAt { get; set; }
}
