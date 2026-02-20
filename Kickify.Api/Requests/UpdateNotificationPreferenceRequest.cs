namespace Kickify.Api.Requests;

public class UpdateNotificationPreferenceRequest
{
    public bool MatchRoom { get; set; }
    public bool Friendship { get; set; }
    public bool Post { get; set; }
}
