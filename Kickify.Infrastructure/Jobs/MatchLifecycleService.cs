using Hangfire;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kickify.Infrastructure.Jobs;

public class MatchLifecycleService : IMatchLifecycleService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MatchLifecycleService> _logger;

    // Thời gian cho phép vote và feedback sau trận đấu
    private static readonly TimeSpan ReviewingPeriod = TimeSpan.FromHours(22);
    // Thời gian sau reviewing period để xử lý post-match (gửi AI + update profile)
    private static readonly TimeSpan PostMatchDelay = TimeSpan.FromHours(1);

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

        // Schedule post-match processing sau 1 tiếng (tổng cộng 23 tiếng sau match end)
        // Gửi feedbacks cho AI sentiment analysis + update player profiles
        var postMatchTime = DateTime.UtcNow.Add(PostMatchDelay);
        SchedulePostMatchProcessing(roomId, postMatchTime);
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

        await SendFeedbacksToAiAsync(roomId, matchFeedbackRepository, sentimentAnalysisService);

        await UpdatePlayerProfilesAsync(room, roomParticipantRepository, playerProfileRepository, unitOfWork);

        _logger.LogInformation("Post-match processing completed for room {RoomId}", roomId);
    }

    /// <summary>
    /// Gửi feedbacks của từng player sang AI sentiment analysis.
    /// Group feedbacks theo reviewee (target player), mỗi lần gửi 1 batch cho 1 player.
    /// </summary>
    private async Task SendFeedbacksToAiAsync(
        Guid roomId,
        IMatchFeedbackRepository matchFeedbackRepository,
        ISentimentAnalysisService sentimentAnalysisService)
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

                await sentimentAnalysisService.SendFeedbacksForAnalysisAsync(request);

                _logger.LogInformation(
                    "Sent {Count} feedbacks for player {PlayerId} in match {MatchId} to AI",
                    playerFeedbacks.Count, targetPlayerId, roomId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending feedbacks to AI for room {RoomId}", roomId);
        }
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
        IPlayerProfileRepository playerProfileRepository,
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
                var profile = await playerProfileRepository.GetByUserIdAsync(participant.UserId);
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

                playerProfileRepository.Update(profile);
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