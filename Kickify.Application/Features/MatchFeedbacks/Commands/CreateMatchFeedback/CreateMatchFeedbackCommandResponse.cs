namespace Kickify.Application.Features.MatchFeedbacks.Commands.CreateMatchFeedback;

public class CreateMatchFeedbackCommandResponse
{
    public Guid FeedbackId { get; set; }
    public Guid MatchId { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid RevieweeId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
