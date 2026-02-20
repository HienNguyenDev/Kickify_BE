using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.NotificationPreferences.Commands.UpdateNotificationPreference;

public class UpdateNotificationPreferenceCommand : ICommand<UpdateNotificationPreferenceCommandResponse>
{
    public bool MatchRoom { get; set; }
    public bool Friendship { get; set; }
    public bool Post { get; set; }
}
