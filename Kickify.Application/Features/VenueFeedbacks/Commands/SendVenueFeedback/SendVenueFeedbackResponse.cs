namespace Kickify.Application.Features.VenueFeedbacks.Commands.SendVenueFeedback;

public record SendVenueFeedbackResponse(Guid VenueFeedbackId, Guid VenueId, DateTime CreatedAt);
