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

        // 1. TÌM BOOKING HỢP LỆ (Lọc 1 phát ra luôn kết quả)
        var eligibleBooking = await _bookingRepository.GetEligibleBookingForVenueReviewAsync(request.VenueId, userId, cancellationToken);

        if (eligibleBooking == null)
        {
            // Trả về chung 1 lỗi tổng quát: User chưa đá ở đây bao giờ, hoặc chưa đá xong, hoặc đã review hết các trận rồi.
            return Result.Failure<CreateVenueReviewResponse>(VenueReviewErrors.NotEligible);
        }

        // Lấy Venue Name từ Include
        var venueName = eligibleBooking.Field.Venue?.VenueName ?? string.Empty;

        // 2. TẠO REVIEW GẮN VỚI BOOKING VỪA TÌM ĐƯỢC
        var review = new VenueReview
        {
            ReviewId = Guid.NewGuid(),
            VenueId = request.VenueId,
            UserId = userId,
            BookingId = eligibleBooking.BookingId, // Lấy BookingId hợp lệ móc ra từ DB
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _venueReviewRepository.AddAsync(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} created review {ReviewId} for venue {VenueId} linked to auto-detected booking {BookingId} with rating {Rating}",
            userId, review.ReviewId, request.VenueId, eligibleBooking.BookingId, request.Rating);

        return Result.Success(new CreateVenueReviewResponse(
            review.ReviewId,
            request.VenueId,
            venueName,
            eligibleBooking.BookingId, // Trả về cho FE biết nó đang review trận nào
            review.Rating,
            review.Comment,
            review.CreatedAt
        ));
    }
}


