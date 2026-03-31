namespace Kickify.Api.Requests;

public class UpdateNotificationPreferenceRequest
{
    public bool MatchRoom { get; set; }
    public bool Friendship { get; set; }
    public bool Post { get; set; }
    /// <summary>Optional: omit on older clients to avoid changing Chat preference.</summary>
    public bool? Chat { get; set; }
}
