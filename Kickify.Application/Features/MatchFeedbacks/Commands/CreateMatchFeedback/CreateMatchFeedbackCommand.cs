using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchFeedbacks.Commands.CreateMatchFeedback;

public class CreateMatchFeedbackCommand : ICommand<CreateMatchFeedbackCommandResponse>
{
    public Guid MatchId { get; set; }
    public Guid RevieweeId { get; set; }
    public List<FeedbackItemDto> Feedbacks { get; set; } = new();
}

public class FeedbackItemDto
{
    public Guid? FeedbackId { get; set; }
    public Guid ReviewerId { get; set; }
    public string? Comment { get; set; }
    public int Rating { get; set; }
}
