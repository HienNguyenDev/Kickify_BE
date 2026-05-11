using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.VenueReviews.Queries.GetAllVenueReviews;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.VenueReviews.Queries.GetVenueOwnerReviews;

public class GetVenueOwnerReviewsQueryHandler : IQueryHandler<GetVenueOwnerReviewsQuery, GetVenueOwnerReviewsResponse>
{
    private readonly IVenueReviewRepository _venueReviewRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public GetVenueOwnerReviewsQueryHandler(
        IVenueReviewRepository venueReviewRepository,
        IUserRepository userRepository,
        IUserContext userContext)
    {
        _venueReviewRepository = venueReviewRepository;
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetVenueOwnerReviewsResponse>> Handle(
        GetVenueOwnerReviewsQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        // Verify user is a VenueOwner
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.Role != UserRole.VenueOwner)
        {
            return Result.Failure<GetVenueOwnerReviewsResponse>(VenueReviewErrors.NotVenueOwner);
        }

        var (items, total) = await _venueReviewRepository.GetByVenueOwnerPagedAsync(
            userId,
            request.VenueId,
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

        var response = new GetVenueOwnerReviewsResponse(
            reviews,
            total,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(total / (double)request.PageSize)
        );

        return Result.Success(response);
    }
}
