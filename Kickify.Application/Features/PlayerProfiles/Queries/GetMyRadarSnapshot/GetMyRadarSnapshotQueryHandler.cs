using System.Text.Json;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.PlayerProfiles.Queries.GetMyRadarSnapshot;

public class GetMyRadarSnapshotQueryHandler : IQueryHandler<GetMyRadarSnapshotQuery, GetMyRadarSnapshotResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetMyRadarSnapshotQueryHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    /// <summary>
    /// Get current user's radar snapshot cached from AI post-match processing.
    /// </summary>
    public async Task<Result<GetMyRadarSnapshotResponse>> Handle(GetMyRadarSnapshotQuery request, CancellationToken cancellationToken)
    {
        var snapshot = await _dbContext.PlayerRadarSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PlayerId == _userContext.UserId, cancellationToken);

        if (snapshot is null)
        {
            return Result.Success(new GetMyRadarSnapshotResponse(
                0, 0, 0, 0, 0,
                new List<RadarAssessmentItem>(),
                string.Empty,
                DateTime.MinValue));
        }

        var assessments = JsonSerializer.Deserialize<List<RadarAssessmentItem>>(snapshot.AssessmentsJson) ?? new List<RadarAssessmentItem>();
        return Result.Success(new GetMyRadarSnapshotResponse(
            snapshot.Form,
            snapshot.WinRate,
            snapshot.CommunityScore,
            snapshot.Trust,
            snapshot.Contribution,
            assessments,
            snapshot.Summary,
            snapshot.UpdatedAt));
    }
}
