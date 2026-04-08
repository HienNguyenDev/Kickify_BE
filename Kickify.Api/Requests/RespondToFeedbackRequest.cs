namespace Kickify.Api.Requests;

public class RespondToFeedbackRequest
{
    /// <summary>
    /// Nội dung phản hồi cho feedback đã nhận.
    /// </summary>
    public string Response { get; set; } = string.Empty;
}
