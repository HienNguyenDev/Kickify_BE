using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.VenueReviews.Queries.GetAllVenueReviews;

public class GetAllVenueReviewsQueryHandler : IQueryHandler<GetAllVenueReviewsQuery, GetAllVenueReviewsResponse>
{
    private readonly IVenueReviewRepository _venueReviewRepository;

    public GetAllVenueReviewsQueryHandler(IVenueReviewRepository venueReviewRepository)
    {
        _venueReviewRepository = venueReviewRepository;
    }

    public async Task<Result<GetAllVenueReviewsResponse>> Handle(
        GetAllVenueReviewsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _venueReviewRepository.GetAllPagedAsync(
            request.VenueId,
            request.UserId,
            request.MinRating,
            request.MaxRating,
            request.Page,
            request.PageSize,
            cancellationToken);

        var reviews = items.Select(r => new VenueReviewItemDto(
            r.ReviewId,
            r.VenueId,
            r.Venue?.VenueName ?? string.Empty,
            r.UserId,
            r.User?.FullName,
            r.User?.AvatarUrl,
            r.BookingId,
            r.Rating,
            r.Comment,
            r.OwnerResponse,
            r.ResponseDate,
            r.CreatedAt
        )).ToList();

        var response = new GetAllVenueReviewsResponse(
            reviews,
            total,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(total / (double)request.PageSize)
        );

        return Result.Success(response);
    }
}
