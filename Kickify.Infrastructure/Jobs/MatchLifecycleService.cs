using Hangfire;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Kickify.Infrastructure.Jobs;

public class MatchLifecycleService : IMatchLifecycleService
{
    private const string AiContractVersion = "2026-04-elo-radar-v1";
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MatchLifecycleService> _logger;

    // Thời gian cho phép vote và feedback sau trận đấu
    private static readonly TimeSpan ReviewingPeriod = TimeSpan.FromHours(22);

    public MatchLifecycleService(
        IBackgroundJobClient backgroundJobClient,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MatchLifecycleService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public void ScheduleMatchStart(Guid roomId, DateTime matchStartTime)
    {
        var delay = matchStartTime - DateTime.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        var jobId = _backgroundJobClient.Schedule(
            () => StartMatchAsync(roomId),
            delay);

        UpdateRoomJobId(roomId, r => r.StartMatchJobId = jobId);
        _logger.LogInformation("Scheduled match start for room {RoomId} at {StartTime}, JobId: {JobId}",
            roomId, matchStartTime, jobId);
    }

    public void ScheduleMatchEnd(Guid roomId, DateTime matchEndTime)
    {
        var delay = matchEndTime - DateTime.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        var jobId = _backgroundJobClient.Schedule(
            () => EndMatchAsync(roomId),
            delay);

        UpdateRoomJobId(roomId, r => r.EndMatchJobId = jobId);
        _logger.LogInformation("Scheduled match end for room {RoomId} at {EndTime}, JobId: {JobId}",
            roomId, matchEndTime, jobId);
    }

    public void ScheduleReviewingPeriodEnd(Guid roomId, DateTime closeTime)
    {
        var delay = closeTime - DateTime.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        var jobId = _backgroundJobClient.Schedule(
            () => CloseReviewingPeriodAsync(roomId),
            delay);

        UpdateRoomJobId(roomId, r => r.FinalizeResultJobId = jobId);
        _logger.LogInformation("Scheduled reviewing period end for room {RoomId} at {CloseTime}, JobId: {JobId}",
            roomId, closeTime, jobId);
    }

    public void SchedulePostMatchProcessing(Guid roomId, DateTime processTime)
    {
        var delay = processTime - DateTime.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        var jobId = _backgroundJobClient.Schedule(
            () => ProcessPostMatchAsync(roomId),
            delay);

        _logger.LogInformation(
            "Scheduled post-match processing for room {RoomId} at {ProcessTime}, JobId: {JobId}",
            roomId, processTime, jobId);
    }

    public void SchedulePreMatchReminders(Guid roomId, DateTime matchStartTime)
    {
        var now = DateTime.UtcNow;
        var fire60 = matchStartTime.AddMinutes(-60);
        var fire30 = matchStartTime.AddMinutes(-30);

        if (fire60 > now)
        {
            var delay = fire60 - now;
            _backgroundJobClient.Schedule(() => PreMatchReminderAsync(roomId, 60), delay);
            _logger.LogInformation("Scheduled 60m pre-match reminder for room {RoomId} in {Delay}", roomId, delay);
        }

        if (fire30 > now)
        {
            var delay = fire30 - now;
            _backgroundJobClient.Schedule(() => PreMatchReminderAsync(roomId, 30), delay);
            _logger.LogInformation("Scheduled 30m pre-match reminder for room {RoomId} in {Delay}", roomId, delay);
        }
    }

    public async Task PreMatchReminderAsync(Guid roomId, int minutesBefore)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        await publisher.Publish(new PreMatchReminderRequestedDomainEvent(roomId, minutesBefore), CancellationToken.None);
    }

    public async Task PostMatchVoteFeedbackReminderAsync(Guid roomId, int attempt)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        await publisher.Publish(new PostMatchVoteFeedbackReminderRequestedDomainEvent(roomId, attempt), CancellationToken.None);
    }

    public void CancelAllJobs(string? startJobId, string? endJobId, string? finalizeJobId)
    {
        if (!string.IsNullOrEmpty(startJobId))
            _backgroundJobClient.Delete(startJobId);
        if (!string.IsNullOrEmpty(endJobId))
            _backgroundJobClient.Delete(endJobId);
        if (!string.IsNullOrEmpty(finalizeJobId))
            _backgroundJobClient.Delete(finalizeJobId);
    }

    public async Task StartMatchAsync(Guid roomId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = await matchRoomRepository.GetByIdAsync(roomId);
        if (room == null)
        {
            _logger.LogWarning("Room {RoomId} not found for match start", roomId);
            return;
        }

        if (room.Status != RoomStatus.Locked)
        {
            _logger.LogWarning("Room {RoomId} is not in Locked status, cannot start match. Current status: {Status}",
                roomId, room.Status);
            return;
        }

        // ==========================================
        // ESCROW RELEASE: Chuyển tiền cho Venue Owner khi trận đấu chính thức bắt đầu
        // ==========================================
        await TransferFundsToVenueOwnerAsync(room, scope.ServiceProvider);
        // ==========================================

        room.Status = RoomStatus.InProgress;
        room.StartMatchJobId = null;
        matchRoomRepository.Update(room);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation("Match started for room {RoomId}. Funds transferred to Venue Owner.", roomId);
        // Schedule match end
        var matchEndTime = room.MatchDate.Add(room.StartTime).AddMinutes(room.DurationMinutes);
        ScheduleMatchEnd(roomId, matchEndTime);
    }

    public async Task EndMatchAsync(Guid roomId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = await matchRoomRepository.GetByIdAsync(roomId);
        if (room == null)
        {
            _logger.LogWarning("Room {RoomId} not found for match end", roomId);
            return;
        }

        if (room.Status != RoomStatus.InProgress)
        {
            _logger.LogWarning("Room {RoomId} is not in InProgress status, cannot end match. Current status: {Status}",
                roomId, room.Status);
            return;
        }

        room.Status = RoomStatus.Reviewing;
        room.EndMatchJobId = null;
        matchRoomRepository.Update(room);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation("Match ended for room {RoomId}, entering 22-hour reviewing phase for voting and feedback", roomId);

        // Schedule đóng reviewing period sau 22 tiếng
        var closeTime = DateTime.UtcNow.Add(ReviewingPeriod);
        ScheduleReviewingPeriodEnd(roomId, closeTime);

        _backgroundJobClient.Schedule(() => PostMatchVoteFeedbackReminderAsync(roomId, 1), TimeSpan.FromMinutes(15));
        _backgroundJobClient.Schedule(() => PostMatchVoteFeedbackReminderAsync(roomId, 2), TimeSpan.FromMinutes(30));
        _logger.LogInformation("Scheduled vote/feedback reminders (+15m, +30m) for room {RoomId}", roomId);
    }

    /// <summary>
    /// Đóng giai đoạn reviewing sau 22 tiếng, không cho vote và feedback nữa
    /// </summary>
    public async Task CloseReviewingPeriodAsync(Guid roomId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var matchResultVoteRepository = scope.ServiceProvider.GetRequiredService<IMatchResultVoteRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = await matchRoomRepository.GetByIdAsync(roomId);
        if (room == null)
        {
            _logger.LogWarning("Room {RoomId} not found for closing reviewing period", roomId);
            return;
        }

        if (room.Status != RoomStatus.Reviewing)
        {
            _logger.LogWarning("Room {RoomId} is not in Reviewing status, cannot close. Current status: {Status}",
                roomId, room.Status);
            return;
        }

        // Lấy tất cả votes và tính kết quả cuối cùng
        var votes = await matchResultVoteRepository.GetVotesByRoomAsync(roomId);

        if (votes.Count > 0)
        {
            // Tìm kết quả có nhiều vote nhất
            var winningResult = votes
                .GroupBy(v => v.Vote)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;

            room.FinalResult = winningResult;
            room.ResultConfirmedBy = votes.Count;

            _logger.LogInformation("Room {RoomId} final result determined: {Result} with {VoteCount} votes",
                roomId, winningResult, votes.Count);
        }
        else
        {
            _logger.LogInformation("Room {RoomId} completed with no votes submitted", roomId);
        }

        // Chuyển sang Completed - không cho vote và feedback nữa
        room.Status = RoomStatus.Completed;
        room.FinalizeResultJobId = null;

        matchRoomRepository.Update(room);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation("Room {RoomId} reviewing period closed after 22 hours. Status changed to Completed.", roomId);

        // Trigger post-match processing ngay khi reviewing đóng.
        _backgroundJobClient.Enqueue(() => ProcessPostMatchAsync(roomId));
    }

    public async Task TryFinalizeReviewingWhenAllVotesAsync(Guid roomId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var matchResultVoteRepository = scope.ServiceProvider.GetRequiredService<IMatchResultVoteRepository>();

        var room = await matchRoomRepository.GetByIdAsync(roomId);
        if (room is null || room.Status != RoomStatus.Reviewing)
        {
            return;
        }

        var voteCount = await matchResultVoteRepository.GetVoteCountByRoomAsync(roomId, CancellationToken.None);
        if (voteCount < room.FilledSlots || room.FilledSlots <= 0)
        {
            return;
        }

        CancelAllJobs(null, null, room.FinalizeResultJobId);
        await CloseReviewingPeriodAsync(roomId);
    }

    /// <summary>
    /// Xử lý sau trận đấu (23 tiếng sau match end):
    /// 1. Gửi tất cả feedback của từng player sang AI để sentiment analysis
    /// 2. Update player profiles dựa trên kết quả trận đấu (win/loss/draw)
    /// </summary>
    public async Task ProcessPostMatchAsync(Guid roomId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var matchFeedbackRepository = scope.ServiceProvider.GetRequiredService<IMatchFeedbackRepository>();
        var playerProfileRepository = scope.ServiceProvider.GetRequiredService<IPlayerProfileRepository>();
        var roomParticipantRepository = scope.ServiceProvider.GetRequiredService<IRoomParticipantRepository>();
        var sentimentAnalysisService = scope.ServiceProvider.GetRequiredService<ISentimentAnalysisService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = await matchRoomRepository.GetByIdAsync(roomId);
        if (room == null)
        {
            _logger.LogWarning("Room {RoomId} not found for post-match processing", roomId);
            return;
        }

        if (room.Status != RoomStatus.Completed)
        {
            _logger.LogWarning("Room {RoomId} is not in Completed status, cannot process post-match. Current status: {Status}",
                roomId, room.Status);
            return;
        }

        _logger.LogInformation("Starting post-match processing for room {RoomId}", roomId);

        await SendFeedbacksToAiAsync(roomId, matchFeedbackRepository, sentimentAnalysisService, unitOfWork);

        await CalculateEloAndRadarWithAiAsync(
            room,
            matchFeedbackRepository,
            roomParticipantRepository,
            playerProfileRepository,
            sentimentAnalysisService,
            dbContext,
            unitOfWork);

        await UpdatePlayerProfilesAsync(room, roomParticipantRepository, dbContext, unitOfWork);

        _logger.LogInformation("Post-match processing completed for room {RoomId}", roomId);
    }

    /// <summary>
    /// Gửi feedbacks của từng player sang AI sentiment analysis.
    /// Group feedbacks theo reviewee (target player), mỗi lần gửi 1 batch cho 1 player.
    /// </summary>
    private async Task SendFeedbacksToAiAsync(
        Guid roomId,
        IMatchFeedbackRepository matchFeedbackRepository,
        ISentimentAnalysisService sentimentAnalysisService,
        IUnitOfWork unitOfWork)
    {
        try
        {
            var allFeedbacks = await matchFeedbackRepository.GetFeedbacksByMatchAsync(roomId);

            if (allFeedbacks.Count == 0)
            {
                _logger.LogInformation("No feedbacks found for room {RoomId}, skipping AI analysis", roomId);
                return;
            }

            // Group feedbacks theo reviewee (target player)
            var feedbacksByPlayer = allFeedbacks.GroupBy(f => f.RevieweeId);

            foreach (var group in feedbacksByPlayer)
            {
                var targetPlayerId = group.Key;
                var playerFeedbacks = group.ToList();

                var request = new SentimentBatchRequest(
                    MatchId: roomId.ToString(),
                    TargetPlayerId: targetPlayerId.ToString(),
                    Feedbacks: playerFeedbacks.Select(f => new SentimentFeedbackItem(
                        FeedbackId: f.FeedbackId.ToString(),
                        ReviewerId: f.ReviewerId.ToString(),
                        Comment: f.Comment,
                        StarRating: f.Rating
                    )).ToList()
                );

                var sentimentResponse = await sentimentAnalysisService.SendFeedbacksForAnalysisAsync(request);
                if (sentimentResponse is not null && sentimentResponse.SentimentDetails.Count > 0)
                {
                    var feedbackById = playerFeedbacks.ToDictionary(x => x.FeedbackId.ToString(), x => x);
                    foreach (var detail in sentimentResponse.SentimentDetails)
                    {
                        if (!feedbackById.TryGetValue(detail.FeedbackId, out var feedback))
                        {
                            continue;
                        }

                        feedback.SentimentScore = detail.Score;
                        feedback.SentimentLabel = detail.Label.ToLowerInvariant() switch
                        {
                            "positive" => SentimentLabel.Positive,
                            "negative" => SentimentLabel.Negative,
                            _ => SentimentLabel.Neutral
                        };

                        matchFeedbackRepository.Update(feedback);
                    }
                }

                _logger.LogInformation(
                    "Sent {Count} feedbacks for player {PlayerId} in match {MatchId} to AI",
                    playerFeedbacks.Count, targetPlayerId, roomId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending feedbacks to AI for room {RoomId}", roomId);
        }

        await unitOfWork.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>
    /// Call AI service to calculate ELO and radar analysis, then persist ELO history.
    /// </summary>
    private async Task CalculateEloAndRadarWithAiAsync(
        MatchRoom room,
        IMatchFeedbackRepository matchFeedbackRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IPlayerProfileRepository playerProfileRepository,
        ISentimentAnalysisService sentimentAnalysisService,
        IApplicationDbContext dbContext,
        IUnitOfWork unitOfWork)
    {
        var participants = await roomParticipantRepository.GetParticipantsByRoomAsync(room.RoomId);
        if (participants.Count == 0 || room.FinalResult is null)
        {
            return;
        }

        var allFeedbacks = await matchFeedbackRepository.GetFeedbacksByMatchAsync(room.RoomId);
        var teamAElo = await CalculateAverageTeamEloAsync(participants, TeamAssignment.A, playerProfileRepository);
        var teamBElo = await CalculateAverageTeamEloAsync(participants, TeamAssignment.B, playerProfileRepository);
        var top50UserIds = await dbContext.PlayerProfiles
            .AsNoTracking()
            .OrderByDescending(x => x.CurrentElo)
            .ThenByDescending(x => x.TotalMatches)
            .Take(50)
            .Select(x => x.UserId)
            .ToListAsync();
        var top50Set = top50UserIds.ToHashSet();
        var activeEloConfig = await dbContext.EloConfigurations
            .AsNoTracking()
            .Where(x => x.IsActive && x.EffectiveFrom <= DateTime.UtcNow.Date && (x.EffectiveTo == null || x.EffectiveTo >= DateTime.UtcNow.Date))
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync();

        if (activeEloConfig is null)
        {
            _logger.LogError("No active EloConfiguration found. Skipping ELO/Radar AI processing for room {RoomId}", room.RoomId);
            return;
        }

        foreach (var participant in participants)
        {
            var profile = await dbContext.PlayerProfiles.FirstOrDefaultAsync(x => x.UserId == participant.UserId);
            if (profile is null)
            {
                continue;
            }

            var teamResult = ResolveTeamResult(room.FinalResult.Value, participant.TeamAssignment);
            var feedbackForPlayer = allFeedbacks.Where(x => x.RevieweeId == participant.UserId).ToList();
            var (recentTotal, recentWins) = await GetRecentMatchesStatsAsync(room.RoomId, participant.UserId, dbContext);
            var daysSinceLastMatch = await GetDaysSinceLastMatchAsync(room.RoomId, participant.UserId, dbContext);
            var myFeedbackAsReviewer = allFeedbacks.Where(x => x.ReviewerId == participant.UserId).ToList();
            var submittedFeedback = myFeedbackAsReviewer.Count > 0;
            var teammateCount = participants.Count(x => x.TeamAssignment == participant.TeamAssignment) - 1;
            var reviewedTeammateCount = myFeedbackAsReviewer
                .Select(x => x.RevieweeId)
                .Distinct()
                .Count();
            var fullFeedbackCoverage = teammateCount > 0 && reviewedTeammateCount >= teammateCount;
            // Send top-50 membership; AI applies the elo >= 2001 condition.
            var isLegend = top50Set.Contains(participant.UserId);

            var eloRequest = new EloCalculationRequest(
                AiContractVersion,
                new EloCalculationMatch(
                    room.RoomId.ToString(),
                    teamResult,
                    participant.TeamAssignment == TeamAssignment.A ? teamAElo : teamBElo,
                    participant.TeamAssignment == TeamAssignment.A ? teamBElo : teamAElo,
                    daysSinceLastMatch),
                new EloCalculationPlayer(
                    participant.UserId.ToString(),
                    profile.CurrentElo,
                    profile.TrustScore,
                    isLegend,
                    new EloRecentMatches(recentTotal, recentWins),
                    new EloContribution(
                        submittedFeedback,
                        fullFeedbackCoverage)),
                new EloKFactors(
                    activeEloConfig.K1MatchResult,
                    activeEloConfig.K2FeedbackSentiment,
                    activeEloConfig.K3WinRate,
                    activeEloConfig.K4Contribution,
                    activeEloConfig.K5Trust),
                feedbackForPlayer.Select(f => new EloCalculationFeedback(
                    f.ReviewerId.ToString(),
                    f.Rating,
                    f.Comment)).ToList());

            var eloResponse = await sentimentAnalysisService.CalculateEloAsync(eloRequest);
            if (eloResponse is not null && ValidateEloResponse(eloResponse, profile.CurrentElo, _logger))
            {
                var eloBefore = profile.CurrentElo;
                profile.CurrentElo = eloResponse.Elo.New;
                profile.CurrentRank = eloResponse.Rank.New;
                profile.IsLegend = string.Equals(eloResponse.Rank.New, "Legend", StringComparison.Ordinal);
                dbContext.PlayerProfiles.Update(profile);

                var existingEloHistory = await dbContext.EloHistories
                    .FirstOrDefaultAsync(x => x.MatchId == room.RoomId && x.UserId == participant.UserId);

                if (existingEloHistory is null)
                {
                    existingEloHistory = new EloHistory
                    {
                        EloHistoryId = Guid.NewGuid(),
                        UserId = participant.UserId,
                        MatchId = room.RoomId,
                        CreatedAt = DateTime.UtcNow
                    };
                    dbContext.EloHistories.Add(existingEloHistory);
                }

                existingEloHistory.EloBefore = eloBefore;
                existingEloHistory.EloAfter = eloResponse.Elo.New;
                existingEloHistory.EloChange = eloResponse.Elo.New - eloBefore;
                existingEloHistory.K1MatchResultComponent = eloResponse.Breakdown.K1MatchResult.Delta;
                existingEloHistory.K2FeedbackSentimentComponent = eloResponse.Breakdown.K2FeedbackSentiment.Delta;
                existingEloHistory.K3WinRateComponent = eloResponse.Breakdown.K3WinRate.Delta;
                existingEloHistory.K4ContributionComponent = eloResponse.Breakdown.K4Contribution.Delta;
                existingEloHistory.K5TrustComponent = eloResponse.Breakdown.K5Trust.Delta;
                existingEloHistory.CalculationDetails = JsonSerializer.Serialize(eloResponse.Breakdown);
            }

            var radarRequest = new RadarAnalysisRequest(
                AiContractVersion,
                participant.UserId.ToString(),
                new RadarPlayerProfile(
                    profile.CurrentElo,
                    profile.TrustScore,
                    profile.TotalMatches,
                    profile.Wins,
                    profile.Losses,
                    profile.Draws,
                    profile.WinStreak,
                    0,
                    profile.ReportCount,
                    string.Empty),
                await BuildRecentMatchPayloadAsync(participant.UserId, dbContext),
                feedbackForPlayer.Select(f => new RadarFeedbackReceived(f.Rating, f.Comment, Math.Max(1, (DateTime.UtcNow - f.CreatedAt).Days))).ToList());

            var radarResponse = await sentimentAnalysisService.AnalyzeRadarAsync(radarRequest);
            if (radarResponse is not null && ValidateRadarResponse(radarResponse, _logger))
            {
                var existingSnapshot = await dbContext.PlayerRadarSnapshots
                    .FirstOrDefaultAsync(x => x.PlayerId == participant.UserId);

                if (existingSnapshot is null)
                {
                    existingSnapshot = new PlayerRadarSnapshot
                    {
                        PlayerId = participant.UserId
                    };
                    dbContext.PlayerRadarSnapshots.Add(existingSnapshot);
                }

                existingSnapshot.Form = radarResponse.Radar.Form;
                existingSnapshot.WinRate = radarResponse.Radar.WinRate;
                existingSnapshot.CommunityScore = radarResponse.Radar.CommunityScore;
                existingSnapshot.Trust = radarResponse.Radar.Trust;
                existingSnapshot.Contribution = radarResponse.Radar.Contribution;
                existingSnapshot.AssessmentsJson = JsonSerializer.Serialize(radarResponse.Assessments);
                existingSnapshot.Summary = radarResponse.Summary;
                existingSnapshot.UpdatedAt = DateTime.UtcNow;
            }
        }

        await unitOfWork.SaveChangesAsync(CancellationToken.None);
    }

    private static decimal ResolveTeamResult(MatchResult finalResult, TeamAssignment team)
    {
        return finalResult switch
        {
            MatchResult.TeamAWin when team == TeamAssignment.A => 1.0m,
            MatchResult.TeamBWin when team == TeamAssignment.B => 1.0m,
            MatchResult.Draw => 0.5m,
            _ => 0.0m
        };
    }

    private static async Task<int> CalculateAverageTeamEloAsync(
        IEnumerable<RoomParticipant> participants,
        TeamAssignment team,
        IPlayerProfileRepository playerProfileRepository)
    {
        var teamParticipants = participants.Where(x => x.TeamAssignment == team).ToList();
        if (teamParticipants.Count == 0)
        {
            return 1000;
        }

        var sum = 0;
        foreach (var member in teamParticipants)
        {
            var profile = await playerProfileRepository.GetByUserIdAsync(member.UserId);
            sum += profile?.CurrentElo ?? 1000;
        }

        return (int)Math.Round((double)sum / teamParticipants.Count);
    }

    private static async Task<int> GetDaysSinceLastMatchAsync(Guid currentMatchId, Guid userId, IApplicationDbContext dbContext)
    {
        var previousMatchDate = await dbContext.RoomParticipants
            .Where(x => x.UserId == userId && x.RoomId != currentMatchId)
            .Join(dbContext.MatchRooms, rp => rp.RoomId, rm => rm.RoomId, (rp, rm) => rm.MatchDate)
            .OrderByDescending(x => x)
            .FirstOrDefaultAsync();

        if (previousMatchDate == default)
        {
            return 0;
        }

        return Math.Max(0, (DateTime.UtcNow.Date - previousMatchDate.Date).Days);
    }

    private static async Task<(int Total, int Wins)> GetRecentMatchesStatsAsync(Guid currentMatchId, Guid userId, IApplicationDbContext dbContext)
    {
        var recent = await dbContext.RoomParticipants
            .Where(x => x.UserId == userId && x.RoomId != currentMatchId)
            .Join(
                dbContext.MatchRooms.Where(r => r.Status == RoomStatus.Completed && r.FinalResult != null),
                rp => rp.RoomId,
                rm => rm.RoomId,
                (rp, rm) => new { rp.TeamAssignment, rm.FinalResult, rm.MatchDate, rm.StartTime })
            .OrderByDescending(x => x.MatchDate)
            .ThenByDescending(x => x.StartTime)
            .Take(15)
            .ToListAsync();

        var total = recent.Count;
        var wins = recent.Count(x =>
            (x.FinalResult == MatchResult.TeamAWin && x.TeamAssignment == TeamAssignment.A)
            || (x.FinalResult == MatchResult.TeamBWin && x.TeamAssignment == TeamAssignment.B));

        return (total, wins);
    }

    private static async Task<List<RadarRecentMatch>> BuildRecentMatchPayloadAsync(Guid userId, IApplicationDbContext dbContext)
    {
        var matches = await dbContext.RoomParticipants
            .Where(x => x.UserId == userId)
            .Join(dbContext.MatchRooms, rp => rp.RoomId, rm => rm.RoomId, (rp, rm) => new { rp, rm })
            .Where(x => x.rm.Status == RoomStatus.Completed && x.rm.FinalResult != null)
            .OrderByDescending(x => x.rm.MatchDate)
            .ThenByDescending(x => x.rm.StartTime)
            .Take(15)
            .Select(x => new
            {
                x.rp.RoomId,
                x.rp.TeamAssignment,
                x.rm.FinalResult,
                x.rm.MatchDate
            })
            .ToListAsync();

        if (matches.Count == 0)
        {
            return [];
        }

        var roomIds = matches.Select(m => m.RoomId).Distinct().ToList();

        var participantRows = await dbContext.RoomParticipants
            .Where(x => roomIds.Contains(x.RoomId))
            .Select(x => new { x.RoomId, x.UserId, x.TeamAssignment })
            .ToListAsync();

        var userIds = participantRows.Select(x => x.UserId).Distinct().ToList();
        var currentEloByUser = await dbContext.PlayerProfiles
            .Where(x => userIds.Contains(x.UserId))
            .ToDictionaryAsync(x => x.UserId, x => x.CurrentElo);

        var eloHistoryByRoomAndUser = await dbContext.EloHistories
            .Where(x => roomIds.Contains(x.MatchId))
            .Select(x => new
            {
                x.MatchId,
                x.UserId,
                x.EloBefore
            })
            .ToListAsync();

        var historicalEloLookup = eloHistoryByRoomAndUser
            .GroupBy(x => (x.MatchId, x.UserId))
            .ToDictionary(g => g.Key, g => g.Last().EloBefore);

        var feedbackMatchIds = await dbContext.MatchFeedbacks
            .Where(x => x.ReviewerId == userId && roomIds.Contains(x.MatchId))
            .Select(x => x.MatchId)
            .Distinct()
            .ToListAsync();

        var feedbackSubmitted = feedbackMatchIds.ToHashSet();

        static int TeamAverage(
            Guid roomId,
            IEnumerable<Guid> ids,
            IReadOnlyDictionary<(Guid MatchId, Guid UserId), int> historicalElos,
            Dictionary<Guid, int> fallbackCurrentElos)
        {
            var list = ids
                .Select(userId =>
                {
                    if (historicalElos.TryGetValue((roomId, userId), out var historical))
                    {
                        return historical;
                    }

                    return fallbackCurrentElos.GetValueOrDefault(userId, 1000);
                })
                .ToList();
            var avg = list.Count == 0 ? 1000 : list.Average();
            return (int)Math.Round(avg);
        }

        var list = new List<RadarRecentMatch>();
        foreach (var m in matches)
        {
            if (m.FinalResult is null)
            {
                continue;
            }

            var inRoom = participantRows.Where(x => x.RoomId == m.RoomId).ToList();
            var teamAIds = inRoom.Where(x => x.TeamAssignment == TeamAssignment.A).Select(x => x.UserId);
            var teamBIds = inRoom.Where(x => x.TeamAssignment == TeamAssignment.B).Select(x => x.UserId);
            var ownTeamIds = m.TeamAssignment == TeamAssignment.A ? teamAIds : teamBIds;
            var oppTeamIds = m.TeamAssignment == TeamAssignment.A ? teamBIds : teamAIds;
            var ownElo = TeamAverage(m.RoomId, ownTeamIds, historicalEloLookup, currentEloByUser);
            var oppElo = TeamAverage(m.RoomId, oppTeamIds, historicalEloLookup, currentEloByUser);
            var expected = ComputeEloExpectedScore(ownElo, oppElo);

            var resultDecimal = m.FinalResult.Value switch
            {
                MatchResult.TeamAWin when m.TeamAssignment == TeamAssignment.A => 1.0m,
                MatchResult.TeamBWin when m.TeamAssignment == TeamAssignment.B => 1.0m,
                MatchResult.Draw => 0.5m,
                _ => 0.0m
            };

            var daysAgo = Math.Max(0, (DateTime.UtcNow.Date - m.MatchDate.Date).Days);
            var submitted = feedbackSubmitted.Contains(m.RoomId);

            list.Add(new RadarRecentMatch(
                m.RoomId.ToString(),
                resultDecimal,
                expected,
                daysAgo,
                submitted));
        }

        return list;
    }

    private static decimal ComputeEloExpectedScore(int ownTeamAverageElo, int opponentTeamAverageElo)
    {
        var diff = opponentTeamAverageElo - ownTeamAverageElo;
        return (decimal)(1.0 / (1.0 + Math.Pow(10, diff / 400.0)));
    }

    private static bool ValidateEloResponse(EloCalculationResponse response, int previousElo, ILogger logger)
    {
        if (!string.Equals(response.ContractVersion, AiContractVersion, StringComparison.Ordinal))
        {
            logger.LogWarning("Rejected ELO response: contract version mismatch {ContractVersion}", response.ContractVersion);
            return false;
        }

        var componentSum = response.Breakdown.K1MatchResult.Delta
            + response.Breakdown.K2FeedbackSentiment.Delta
            + response.Breakdown.K3WinRate.Delta
            + response.Breakdown.K4Contribution.Delta
            + response.Breakdown.K5Trust.Delta;

        if (Math.Abs(componentSum - response.Elo.Delta) > 0.01m)
        {
            logger.LogWarning("Rejected ELO response: breakdown sum mismatch. Sum={Sum}, Delta={Delta}", componentSum, response.Elo.Delta);
            return false;
        }

        // Python round() uses banker's rounding (to even).
        var expectedNew = (int)Math.Round(previousElo + response.Elo.Delta, MidpointRounding.ToEven);
        if (response.Elo.New != expectedNew)
        {
            logger.LogWarning("Rejected ELO response: new ELO mismatch. Expected={Expected}, Actual={Actual}", expectedNew, response.Elo.New);
            return false;
        }

        return true;
    }

    private static bool ValidateRadarResponse(RadarAnalysisResponse response, ILogger logger)
    {
        if (!string.Equals(response.ContractVersion, AiContractVersion, StringComparison.Ordinal))
        {
            logger.LogWarning("Rejected radar response: contract version mismatch {ContractVersion}", response.ContractVersion);
            return false;
        }

        var radar = response.Radar;
        var isValid = radar.Form >= -1m && radar.Form <= 1m
            && radar.WinRate >= 0m && radar.WinRate <= 1m
            && radar.CommunityScore >= -1m && radar.CommunityScore <= 1m
            && radar.Trust >= 0m && radar.Trust <= 100m
            && radar.Contribution >= 0m && radar.Contribution <= 1m;

        if (!isValid)
        {
            logger.LogWarning("Rejected radar response: axis out of range for player {PlayerId}", response.PlayerId);
        }

        return isValid;
    }

    /// <summary>
    /// Update player profiles dựa trên kết quả trận đấu.
    /// TeamA thắng → TeamA +1 win, TeamB +1 loss
    /// TeamB thắng → TeamB +1 win, TeamA +1 loss
    /// Draw → tất cả +1 draw
    /// Tất cả players +1 total matches
    /// </summary>
    private async Task UpdatePlayerProfilesAsync(
        MatchRoom room,
        IRoomParticipantRepository roomParticipantRepository,
        IApplicationDbContext dbContext,
        IUnitOfWork unitOfWork)
    {
        try
        {
            var participants = await roomParticipantRepository.GetParticipantsByRoomAsync(room.RoomId);

            if (participants == null || !participants.Any())
            {
                _logger.LogWarning("No participants found for room {RoomId}", room.RoomId);
                return;
            }

            foreach (var participant in participants)
            {
                // Same scoped DbContext as CalculateEloAndRadarWithAiAsync: reuse tracked PlayerProfile
                // instances instead of AsNoTracking + Update (avoids identity map conflict on ProfileId).
                var profile = await dbContext.PlayerProfiles
                    .FirstOrDefaultAsync(p => p.UserId == participant.UserId, CancellationToken.None);
                if (profile == null)
                {
                    _logger.LogWarning("Player profile not found for user {UserId} in room {RoomId}",
                        participant.UserId, room.RoomId);
                    continue;
                }

                // +1 total matches cho tất cả
                profile.TotalMatches += 1;

                if (room.FinalResult != null)
                {
                    switch (room.FinalResult)
                    {
                        case MatchResult.TeamAWin:
                            if (participant.TeamAssignment == TeamAssignment.A)
                            {
                                profile.Wins += 1;
                                profile.WinStreak += 1;
                                if (profile.WinStreak > profile.MaxWinStreak)
                                    profile.MaxWinStreak = profile.WinStreak;
                            }
                            else if (participant.TeamAssignment == TeamAssignment.B)
                            {
                                profile.Losses += 1;
                                profile.WinStreak = 0;
                            }
                            break;

                        case MatchResult.TeamBWin:
                            if (participant.TeamAssignment == TeamAssignment.B)
                            {
                                profile.Wins += 1;
                                profile.WinStreak += 1;
                                if (profile.WinStreak > profile.MaxWinStreak)
                                    profile.MaxWinStreak = profile.WinStreak;
                            }
                            else if (participant.TeamAssignment == TeamAssignment.A)
                            {
                                profile.Losses += 1;
                                profile.WinStreak = 0;
                            }
                            break;

                        case MatchResult.Draw:
                            profile.Draws += 1;
                            profile.WinStreak = 0;
                            break;
                    }
                }
            }

            await unitOfWork.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation(
                "Updated player profiles for room {RoomId}. Result: {Result}, Participants: {Count}",
                room.RoomId, room.FinalResult, participants.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating player profiles for room {RoomId}", room.RoomId);
        }
    }

    private void UpdateRoomJobId(Guid roomId, Action<MatchRoom> updateAction)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = matchRoomRepository.GetByIdAsync(roomId).GetAwaiter().GetResult();
        if (room != null)
        {
            updateAction(room);
            matchRoomRepository.Update(room);
            unitOfWork.SaveChangesAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Giải ngân tiền cọc cho Chủ sân (Venue Owner) khi trận đấu bắt đầu.
    /// Hệ thống hoạt động theo mô hình Escrow: Giữ tiền hộ đến khi dịch vụ thực sự diễn ra.
    /// </summary>
    private async Task TransferFundsToVenueOwnerAsync(MatchRoom room, IServiceProvider serviceProvider)
    {
        if (!room.FieldId.HasValue || room.TotalDepositCollected <= 0)
        {
            return; // Không có sân hoặc không có tiền để chuyển
        }

        var fieldRepository = serviceProvider.GetRequiredService<IFieldRepository>();
        var venueRepository = serviceProvider.GetRequiredService<IVenueRepository>();
        var walletRepository = serviceProvider.GetRequiredService<IWalletRepository>();
        var walletTransactionRepository = serviceProvider.GetRequiredService<IWalletTransactionRepository>();

        try
        {
            // 1. Lấy thông tin Sân -> Chủ sân
            var field = await fieldRepository.GetFieldWithVenueAsync(room.FieldId.Value, CancellationToken.None);
            if (field == null) return;

            var venue = await venueRepository.GetByIdAsync(field.VenueId);
            if (venue == null) return;

            // 2. Lấy Ví của Chủ sân
            var ownerWallet = await walletRepository.GetByUserIdAsync(venue.OwnerId, CancellationToken.None);
            if (ownerWallet == null)
            {
                _logger.LogError("CRITICAL: Wallet not found for VenueOwner {OwnerId}. Cannot transfer funds for Room {RoomId}", venue.OwnerId, room.RoomId);
                return;
            }

            // 3. Thực hiện cộng tiền
            var transferAmount = room.TotalDepositCollected;
            ownerWallet.Balance += transferAmount;
            walletRepository.Update(ownerWallet);

            // 4. Ghi log lịch sử giao dịch (Biến TransactionType.BookingIncome nhớ đảm bảo đã có trong Enum của bạn)
            var transaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid(),
                WalletId = ownerWallet.WalletId,
                TransactionType = TransactionType.BookingIncome,
                Amount = transferAmount,
                BalanceAfter = ownerWallet.Balance,
                ReferenceId = room.RoomId, // Có thể link tới BookingId nếu bạn include Booking vào Room
                Description = $"Booking income from room {room.RoomName ?? room.RoomId.ToString()} (Match Started)",
                CreatedAt = DateTime.UtcNow
            };

            await walletTransactionRepository.AddAsync(transaction);

            _logger.LogInformation("Prepared Escrow Release of {Amount} to VenueOwner {OwnerId} for Room {RoomId}", transferAmount, venue.OwnerId, room.RoomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prepare fund transfer for VenueOwner. RoomId: {RoomId}", room.RoomId);
            throw; // Ném lỗi ra ngoài để UnitOfWork không Commit Db, đảm bảo an toàn giao dịch
        }
    }
}