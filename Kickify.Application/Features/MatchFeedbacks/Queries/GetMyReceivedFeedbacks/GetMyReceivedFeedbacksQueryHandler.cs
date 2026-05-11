using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.MatchFeedbacks.Queries.GetMyReceivedFeedbacks;

public class GetMyReceivedFeedbacksQueryHandler : IQueryHandler<GetMyReceivedFeedbacksQuery, GetMyReceivedFeedbacksResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetMyReceivedFeedbacksQueryHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    /// <summary>
    /// Get feedback list received by current player with filtering.
    /// </summary>
    public async Task<Result<GetMyReceivedFeedbacksResponse>> Handle(GetMyReceivedFeedbacksQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MatchFeedbacks
            .AsNoTracking()
            .Where(x => x.RevieweeId == _userContext.UserId);

        if (request.FromDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= request.ToDate.Value);
        }

        if (request.Rating.HasValue)
        {
            query = query.Where(x => x.Rating == request.Rating.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new ReceivedFeedbackItem(
                x.FeedbackId,
                x.MatchId,
                x.ReviewerId,
                x.Reviewer.FullName,
                x.Reviewer.AvatarUrl,
                x.Rating,
                x.Comment,
                x.SentimentScore,
                x.SentimentLabel != null ? x.SentimentLabel.ToString() : null,
                x.RevieweeResponse,
                x.ResponseDate,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetMyReceivedFeedbacksResponse(
            items, total, request.Page, request.PageSize, totalPages));
    }
}
