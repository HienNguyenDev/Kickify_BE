using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.VenueReviews.Commands.ReplyVenueReview;

public class ReplyVenueReviewCommandHandler : ICommandHandler<ReplyVenueReviewCommand, ReplyVenueReviewResponse>
{
    private readonly IVenueReviewRepository _venueReviewRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReplyVenueReviewCommandHandler> _logger;

    public ReplyVenueReviewCommandHandler(
        IVenueReviewRepository venueReviewRepository,
        IUserRepository userRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        ILogger<ReplyVenueReviewCommandHandler> logger)
    {
        _venueReviewRepository = venueReviewRepository;
        _userRepository = userRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ReplyVenueReviewResponse>> Handle(
        ReplyVenueReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.Role != UserRole.VenueOwner)
            return Result.Failure<ReplyVenueReviewResponse>(VenueReviewErrors.NotVenueOwner);

        var review = await _venueReviewRepository.GetByIdWithVenueForUpdateAsync(request.ReviewId, cancellationToken);
        if (review is null)
            return Result.Failure<ReplyVenueReviewResponse>(VenueReviewErrors.NotFound(request.ReviewId));

        if (review.Venue.OwnerId != userId)
            return Result.Failure<ReplyVenueReviewResponse>(VenueReviewErrors.NotOwnerOfVenue);

        if (!string.IsNullOrWhiteSpace(review.OwnerResponse))
            return Result.Failure<ReplyVenueReviewResponse>(VenueReviewErrors.AlreadyReplied);

        var trimmed = request.Message.Trim();
        if (trimmed.Length == 0)
            return Result.Failure<ReplyVenueReviewResponse>(VenueReviewErrors.ReplyRequired);

        var responseAt = DateTime.UtcNow;
        review.OwnerResponse = trimmed;
        // ResponseDate column is 'timestamp without time zone'; Npgsql rejects DateTimeKind.Utc.
        review.ResponseDate = DateTime.SpecifyKind(responseAt, DateTimeKind.Unspecified);

        _venueReviewRepository.Update(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Venue owner {OwnerId} replied to review {ReviewId} for venue {VenueId}",
            userId, review.ReviewId, review.VenueId);

        return Result.Success(new ReplyVenueReviewResponse(
            review.ReviewId,
            review.VenueId,
            review.OwnerResponse,
            responseAt));
    }
}
