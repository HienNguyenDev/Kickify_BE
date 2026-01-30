using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchFeedbacks.Commands.CreateMatchFeedback;

public class CreateMatchFeedbackCommand : ICommand<CreateMatchFeedbackCommandResponse>
{
    public Guid MatchId { get; set; }
    public Guid RevieweeId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
