using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.VenueReviews.Commands.DeleteVenueReview;

public class DeleteVenueReviewCommandHandler : ICommandHandler<DeleteVenueReviewCommand, DeleteVenueReviewResponse>
{
    private readonly IVenueReviewRepository _venueReviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteVenueReviewCommandHandler> _logger;

    public DeleteVenueReviewCommandHandler(
        IVenueReviewRepository venueReviewRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteVenueReviewCommandHandler> logger)
    {
        _venueReviewRepository = venueReviewRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<DeleteVenueReviewResponse>> Handle(
        DeleteVenueReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _venueReviewRepository.GetByIdWithDetailsAsync(request.ReviewId, cancellationToken);
        if (review is null)
        {
            return Result.Failure<DeleteVenueReviewResponse>(VenueReviewErrors.NotFound(request.ReviewId));
        }

        // Soft delete via EF interceptor (sets DeletedAt in SaveChangesAsync)
        _venueReviewRepository.Remove(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("VenueReview soft-deleted: {ReviewId} for Venue {VenueId}", review.ReviewId, review.VenueId);

        return Result.Success(new DeleteVenueReviewResponse(review.ReviewId, true));
    }
}
