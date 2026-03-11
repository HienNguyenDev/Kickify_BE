using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.VenueFeedbacks.Commands.SendVenueFeedback;

internal sealed class SendVenueFeedbackCommandHandler : ICommandHandler<SendVenueFeedbackCommand, SendVenueFeedbackResponse>
{
    private readonly IVenueRepository _venueRepository;
    private readonly IVenueFeedbackRepository _feedbackRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public SendVenueFeedbackCommandHandler(
        IVenueRepository venueRepository,
        IVenueFeedbackRepository feedbackRepository,
        INotificationRepository notificationRepository,
        IPushNotificationService pushNotificationService,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _venueRepository = venueRepository;
        _feedbackRepository = feedbackRepository;
        _notificationRepository = notificationRepository;
        _pushNotificationService = pushNotificationService;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SendVenueFeedbackResponse>> Handle(
        SendVenueFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        var venue = await _venueRepository.GetVenueForUpdateAsync(request.VenueId, cancellationToken);
        if (venue is null)
            return Result.Failure<SendVenueFeedbackResponse>(VenueErrors.NotFound(request.VenueId));

        if (venue.OwnerId == _userContext.UserId)
            return Result.Failure<SendVenueFeedbackResponse>(VenueFeedbackErrors.CannotFeedbackOwnVenue);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        var feedback = new VenueFeedback
        {
            VenueFeedbackId = Guid.NewGuid(),
            VenueId = request.VenueId,
            SenderId = _userContext.UserId,
            Message = request.Message,
            Rating = request.Rating,
            CreatedAt = now
        };

        await _feedbackRepository.AddAsync(feedback);

        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = venue.OwnerId,
            SenderId = _userContext.UserId,
            NotificationType = NotificationType.VenueFeedback,
            Title = "New Venue Feedback",
            Message = $"You received new feedback for venue '{venue.VenueName}'.",
            CreatedAt = now
        };

        await _notificationRepository.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _pushNotificationService.SendToUserAsync(
            venue.OwnerId,
            notification.Title,
            notification.Message,
            new Dictionary<string, string>
            {
                ["type"] = NotificationType.VenueFeedback.ToString(),
                ["venueId"] = venue.VenueId.ToString()
            },
            cancellationToken);

        return Result.Success(new SendVenueFeedbackResponse(
            feedback.VenueFeedbackId, feedback.VenueId, feedback.CreatedAt));
    }
}
