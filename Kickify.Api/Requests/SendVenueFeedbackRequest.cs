namespace Kickify.Api.Requests;

public class SendVenueFeedbackRequest
{
    public string Message { get; set; } = string.Empty;
    public int Rating { get; set; }
}
