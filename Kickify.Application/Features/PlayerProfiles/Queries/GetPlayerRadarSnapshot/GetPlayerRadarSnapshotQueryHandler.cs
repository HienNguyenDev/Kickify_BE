using System.Text.Json;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.PlayerProfiles.Queries.GetMyRadarSnapshot;
using Kickify.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.PlayerProfiles.Queries.GetPlayerRadarSnapshot;

public class GetPlayerRadarSnapshotQueryHandler : IQueryHandler<GetPlayerRadarSnapshotQuery, GetMyRadarSnapshotResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserReader _currentUserReader;
    private readonly IUserRepository _userRepository;

    public GetPlayerRadarSnapshotQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserReader currentUserReader,
        IUserRepository userRepository)
    {
        _dbContext = dbContext;
        _currentUserReader = currentUserReader;
        _userRepository = userRepository;
    }

    public async Task<Result<GetMyRadarSnapshotResponse>> Handle(
        GetPlayerRadarSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = await _dbContext.PlayerRadarSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PlayerId == request.UserId, cancellationToken);

        if (snapshot is null)
        {
            return Result.Success(EmptyResponse());
        }

        var viewerId = _currentUserReader.TryGetUserId();
        var isOwner = viewerId.HasValue && viewerId.Value == request.UserId;

        // Premium check: only the owner who has active Premium sees AI assessments + summary
        var ownerIsPremium = false;
        if (isOwner && viewerId.HasValue)
        {
            var viewer = await _userRepository.GetByIdAsync(viewerId.Value);
            ownerIsPremium = viewer is { IsPremium: true } &&
                             (viewer.PremiumExpireAt is null || viewer.PremiumExpireAt > DateTime.UtcNow);
        }

        List<RadarAssessmentItem> assessments;
        string summary;
        if (isOwner && ownerIsPremium)
        {
            assessments = JsonSerializer.Deserialize<List<RadarAssessmentItem>>(snapshot.AssessmentsJson)
                ?? new List<RadarAssessmentItem>();
            summary = snapshot.Summary;
        }
        else
        {
            assessments = new List<RadarAssessmentItem>();
            summary = string.Empty; // non-premium / non-owner: hide AI summary
        }

        return Result.Success(new GetMyRadarSnapshotResponse(
            snapshot.Form,
            snapshot.WinRate,
            snapshot.CommunityScore,
            snapshot.Trust,
            snapshot.Contribution,
            assessments,
            summary,
            snapshot.UpdatedAt));
    }

    private static GetMyRadarSnapshotResponse EmptyResponse() =>
        new(
            0, 0, 0, 0, 0,
            new List<RadarAssessmentItem>(),
            string.Empty,
            DateTime.MinValue);
}
