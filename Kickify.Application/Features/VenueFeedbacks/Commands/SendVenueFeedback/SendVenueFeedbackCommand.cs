using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.VenueFeedbacks.Commands.SendVenueFeedback;

public record SendVenueFeedbackCommand(Guid VenueId, string Message, int Rating) : ICommand<SendVenueFeedbackResponse>;
