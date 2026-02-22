using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.VenueReviews.Commands.CreateVenueReview;

public class CreateVenueReviewCommandHandler : ICommandHandler<CreateVenueReviewCommand, CreateVenueReviewResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IVenueReviewRepository _venueReviewRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateVenueReviewCommandHandler> _logger;

    public CreateVenueReviewCommandHandler(
        IBookingRepository bookingRepository,
        IVenueReviewRepository venueReviewRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        ILogger<CreateVenueReviewCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _venueReviewRepository = venueReviewRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CreateVenueReviewResponse>> Handle(CreateVenueReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        // Get booking with Field → Venue, MatchRoom → RoomParticipants
        var booking = await _bookingRepository.GetBookingForReviewValidationAsync(request.BookingId, cancellationToken);
        if (booking is null)
        {
            return Result.Failure<CreateVenueReviewResponse>(VenueReviewErrors.BookingNotFound(request.BookingId));
        }

        // Validation 1: Participation check - user must be a participant in the match room
        var isParticipant = booking.MatchRoom?.RoomParticipants?.Any(rp => rp.UserId == userId) ?? false;
        if (!isParticipant)
        {
            return Result.Failure<CreateVenueReviewResponse>(VenueReviewErrors.NotParticipant);
        }

        // Validation 2: Time check - match must have ended (current time > BookingDate + EndTime)
        var matchEndDateTime = booking.BookingDate.Date + booking.EndTime;
        if (DateTime.UtcNow <= matchEndDateTime)
        {
            return Result.Failure<CreateVenueReviewResponse>(VenueReviewErrors.MatchNotEnded);
        }

        // Validation 3: Status check - Booking must be Confirmed/Completed AND MatchRoom must be Completed
        var isBookingValid = booking.Status == BookingStatus.Confirmed || booking.Status == BookingStatus.Completed;
        //var isRoomCompleted = booking.MatchRoom?.Status == RoomStatus.Completed;
        if (!isBookingValid)
            //|| !isRoomCompleted)
        {
            return Result.Failure<CreateVenueReviewResponse>(VenueReviewErrors.BookingNotCompleted);
        }

        // Validation 4: Anti-spam - user must not have already reviewed this booking
        var hasReviewed = await _venueReviewRepository.HasUserReviewedBookingAsync(userId, request.BookingId, cancellationToken);
        if (hasReviewed)
        {
            return Result.Failure<CreateVenueReviewResponse>(VenueReviewErrors.AlreadyReviewed);
        }

        // Derive VenueId from Booking → Field → Venue
        var venueId = booking.Field.VenueId;
        var venueName = booking.Field.Venue?.VenueName ?? string.Empty;

        // Create the review
        var review = new VenueReview
        {
            ReviewId = Guid.NewGuid(),
            VenueId = venueId,
            UserId = userId,
            BookingId = request.BookingId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _venueReviewRepository.AddAsync(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} created review {ReviewId} for venue {VenueId} via booking {BookingId} with rating {Rating}",
            userId, review.ReviewId, venueId, request.BookingId, request.Rating);

        return Result.Success(new CreateVenueReviewResponse(
            review.ReviewId,
            venueId,
            venueName,
            request.BookingId,
            review.Rating,
            review.Comment,
            review.CreatedAt
        ));
    }
}
