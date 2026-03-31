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

public class RoomAutoCloseService : IRoomAutoCloseService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RoomAutoCloseService> _logger;

    public RoomAutoCloseService(
        IBackgroundJobClient backgroundJobClient,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<RoomAutoCloseService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public void ScheduleAutoClose(Guid roomId, TimeSpan delay)
    {
        var jobId = _backgroundJobClient.Schedule(
            () => CloseRoomAsync(roomId),
            delay);

        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = matchRoomRepository.GetByIdAsync(roomId).GetAwaiter().GetResult();
        if (room != null)
        {
            room.AutoCloseJobId = jobId;
            matchRoomRepository.Update(room);
            unitOfWork.SaveChangesAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    public void CancelAutoClose(string? jobId)
    {
        if (string.IsNullOrEmpty(jobId)) return;
        _backgroundJobClient.Delete(jobId);
    }

    public void RescheduleAutoClose(Guid roomId, string? oldJobId, TimeSpan delay)
    {
        CancelAutoClose(oldJobId);
        ScheduleAutoClose(roomId, delay);
    }

    public async Task CloseRoomAsync(Guid roomId)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var participantRepo = scope.ServiceProvider.GetRequiredService<IRoomParticipantRepository>();
            var walletRepo = scope.ServiceProvider.GetRequiredService<IWalletRepository>();
            var walletTransactionRepo = scope.ServiceProvider.GetRequiredService<IWalletTransactionRepository>();
            var matchRoomHubService = scope.ServiceProvider.GetRequiredService<IMatchRoomHubService>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Get room
            var room = await matchRoomRepository.GetByIdAsync(roomId);
            if (room == null || room.Status != RoomStatus.Open) return;

            // 1. Cancel Room
            room.Status = RoomStatus.Cancelled;
            room.AutoCloseJobId = null;
            matchRoomRepository.Update(room);

            // 2. Cancel Booking
            var booking = await bookingRepository.GetBookingByRoomAsync(roomId, CancellationToken.None);
            if (booking != null && booking.Status == BookingStatus.Pending)
            {
                booking.Status = BookingStatus.Cancelled;
                bookingRepository.Update(booking);
            }

            // 3. Refund players
            var participants = await participantRepo.GetParticipantsByRoomAsync(roomId, CancellationToken.None);
            var notifyUserIds = participants.Select(p => p.UserId).Distinct().ToList();
            var paidParticipants = participants.Where(p => p.DepositPaid).ToList();

            foreach (var p in paidParticipants)
            {
                var refundAmount = p.DepositAmount ?? 0;
                if (refundAmount <= 0) continue;

                var wallet = await walletRepo.GetByUserIdAsync(p.UserId, CancellationToken.None);
                if (wallet != null)
                {
                    wallet.Balance += refundAmount;
                    walletRepo.Update(wallet);

                    var refundTx = new WalletTransaction
                    {
                        TransactionId = Guid.NewGuid(),
                        WalletId = wallet.WalletId,
                        TransactionType = TransactionType.Refund,
                        Amount = refundAmount,
                        BalanceAfter = wallet.Balance,
                        ReferenceId = roomId,
                        Description = $"Refund for auto-closed room {room.RoomName ?? roomId.ToString()}",
                        CreatedAt = DateTime.UtcNow
                    };
                    await walletTransactionRepo.AddAsync(refundTx);
                    
                    _logger.LogInformation("Refunded {Amount} to user {UserId} for auto-closed room {RoomId}",
                        refundAmount, p.UserId, roomId);
                }

                p.DepositPaid = false;
                p.CheckedIn = false;
                p.CheckInTime = null;
                p.DepositAmount = null;
                participantRepo.Update(p);
            }
            
            room.TotalDepositCollected = 0;

            await unitOfWork.SaveChangesAsync(CancellationToken.None);

            if (notifyUserIds.Count > 0)
            {
                var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
                await publisher.Publish(
                    new MatchRoomCancelledNotifyParticipantsDomainEvent(roomId, room.RoomName, notifyUserIds),
                    CancellationToken.None);
            }

            // 4. Notify via SignalR
            await matchRoomHubService.NotifyRoomStatusChangedAsync(
                roomId,
                RoomStatus.Cancelled.ToString(),
                CancellationToken.None);
                
            _logger.LogInformation("Successfully auto-closed room {RoomId} and refunded {Count} participants", 
                roomId, paidParticipants.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while auto-closing room {RoomId}", roomId);
        }
    }
}
