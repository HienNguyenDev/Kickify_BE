using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchFeedbacks.Commands.RespondToFeedback;

public record RespondToFeedbackCommand(Guid FeedbackId, string Response) : ICommand;
