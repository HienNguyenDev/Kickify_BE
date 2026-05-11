namespace Kickify.Api.Requests;

public class CreateMatchFeedbackRequest
{
    public Guid MatchId { get; set; }
    public Guid ReviewerId { get; set; }
    public List<FeedbackItemRequest> Feedbacks { get; set; } = new();
}

public class FeedbackItemRequest
{
    public Guid? FeedbackId { get; set; }
    public Guid RevieweeId { get; set; }
    public string? Comment { get; set; }
    public int Rating { get; set; }
}
