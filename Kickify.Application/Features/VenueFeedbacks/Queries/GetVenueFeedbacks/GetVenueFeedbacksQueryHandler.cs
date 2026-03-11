using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.VenueFeedbacks.Queries.GetVenueFeedbacks;

internal sealed class GetVenueFeedbacksQueryHandler : IQueryHandler<GetVenueFeedbacksQuery, GetVenueFeedbacksResponse>
{
    private readonly IVenueFeedbackRepository _feedbackRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IUserContext _userContext;

    public GetVenueFeedbacksQueryHandler(
        IVenueFeedbackRepository feedbackRepository,
        IVenueRepository venueRepository,
        IUserContext userContext)
    {
        _feedbackRepository = feedbackRepository;
        _venueRepository = venueRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetVenueFeedbacksResponse>> Handle(
        GetVenueFeedbacksQuery request,
        CancellationToken cancellationToken)
    {
        var venue = await _venueRepository.GetVenueForUpdateAsync(request.VenueId, cancellationToken);
        if (venue is null)
            return Result.Failure<GetVenueFeedbacksResponse>(VenueErrors.NotFound(request.VenueId));

        if (venue.OwnerId != _userContext.UserId)
            return Result.Failure<GetVenueFeedbacksResponse>(VenueErrors.Unauthorized);

        var (feedbacks, total) = await _feedbackRepository.GetByVenueIdAsync(
            request.VenueId, request.Page, request.PageSize, cancellationToken);

        var dtos = feedbacks.Select(f => new FeedbackItemDto(
            f.VenueFeedbackId,
            f.SenderId,
            f.Sender?.FullName ?? string.Empty,
            f.Sender?.AvatarUrl ?? string.Empty,
            f.Message,
            f.Rating,
            f.CreatedAt)).ToList();

        var totalPages = (int)Math.Ceiling((double)total / request.PageSize);

        return Result.Success(new GetVenueFeedbacksResponse(
            dtos, total, request.Page, request.PageSize, totalPages));
    }
}
