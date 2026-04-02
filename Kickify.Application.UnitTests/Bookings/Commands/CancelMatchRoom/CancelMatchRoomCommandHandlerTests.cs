using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.MatchRooms.Commands.CancelMatchRoom;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Kickify.Application.UnitTests.MatchRooms.Commands.CancelMatchRoom;

public class CancelMatchRoomCommandHandlerTests
{
    private readonly Mock<IMatchRoomRepository> _matchRoomRepoMock = new();
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IRoomParticipantRepository> _participantRepoMock = new();
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly Mock<IWalletTransactionRepository> _walletTxRepoMock = new();
    private readonly Mock<IFieldRepository> _fieldRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IMatchLifecycleService> _lifecycleMock = new();
    private readonly Mock<IRoomAutoCloseService> _autoCloseMock = new();
    private readonly Mock<IMatchRoomHubService> _hubServiceMock = new();
    private readonly Mock<ILogger<CancelMatchRoomCommandHandler>> _loggerMock = new();

    private readonly CancelMatchRoomCommandHandler _sut; // System Under Test

    public CancelMatchRoomCommandHandlerTests()
    {
        _sut = new CancelMatchRoomCommandHandler(
            _matchRoomRepoMock.Object, _bookingRepoMock.Object, _participantRepoMock.Object,
            _walletRepoMock.Object, _walletTxRepoMock.Object, _fieldRepoMock.Object,
            _unitOfWorkMock.Object, _userContextMock.Object, _lifecycleMock.Object,
            _autoCloseMock.Object, _hubServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNotHost_ReturnsOnlyHostCanCancelError()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid(); // Khác HostId
        var roomId = Guid.NewGuid();

        _userContextMock.Setup(x => x.UserId).Returns(currentUserId);

        var room = new MatchRoom { RoomId = roomId, HostId = hostId, Status = RoomStatus.Locked };
        _matchRoomRepoMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Act
        var result = await _sut.Handle(new CancelMatchRoomCommand(roomId, "Bận việc"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(MatchRoomErrors.OnlyHostCanCancel.Code);
    }

    [Fact]
    public async Task Handle_WhenInNoCancelZone_Under4Hours_ReturnsError()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(hostId);

        // Giả lập trận đấu diễn ra sau 3 tiếng nữa (Nằm trong Vùng Cấm)
        var matchTime = DateTime.UtcNow.AddHours(3);
        var room = new MatchRoom
        {
            RoomId = roomId,
            HostId = hostId,
            Status = RoomStatus.Locked,
            MatchDate = matchTime.Date,
            StartTime = matchTime.TimeOfDay
        };

        _matchRoomRepoMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Act
        var result = await _sut.Handle(new CancelMatchRoomCommand(roomId, "Bận việc"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(MatchRoomErrors.CannotCancelWithin4Hours.Code); // Đảm bảo bạn đã thêm Error này nhé
    }

    [Fact]
    public async Task Handle_InPenaltyZone_WithInsufficientFunds_ReturnsError()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(hostId);

        // Giả lập trận đấu diễn ra sau 10 tiếng (Nằm trong Vùng Phạt 4h - 24h)
        var matchTime = DateTime.UtcNow.AddHours(10);

        var room = new MatchRoom
        {
            RoomId = roomId,
            HostId = hostId,
            Status = RoomStatus.Locked,
            MatchDate = matchTime.Date,
            StartTime = matchTime.TimeOfDay,
            TotalDepositCollected = 500000,
            FieldId = Guid.NewGuid(),
            RoomParticipants = new List<RoomParticipant> { new() { UserId = hostId, DepositPaid = true, DepositAmount = 50000 } }
        };

        _matchRoomRepoMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        // Giả lập lấy thông tin Chủ sân
        _fieldRepoMock.Setup(x => x.GetFieldWithVenueAsync(room.FieldId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Field { Venue = new Venue { OwnerId = Guid.NewGuid() } });

        // Giả lập ví Host không đủ tiền (Tiền phạt là 125k (25%), ví đang có 74,999 + cọc 50k = 124.999 < 125k)
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(hostId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Wallet { Balance = 74999 });

        // Act
        var result = await _sut.Handle(new CancelMatchRoomCommand(roomId, "Hủy bừa"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(WalletErrors.InsufficientBalanceForPenalty.Code);
    }

    [Fact]
    public async Task Handle_InSafeZone_Over24Hours_SuccessWithFullRefundAndNoPenalty()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(hostId);

        // Giả lập trận đấu diễn ra sau 48 tiếng (Vùng an toàn)
        var matchTime = DateTime.UtcNow.AddHours(48);

        var room = new MatchRoom
        {
            RoomId = roomId,
            HostId = hostId,
            Status = RoomStatus.Locked,
            MatchDate = matchTime.Date,
            StartTime = matchTime.TimeOfDay,
            TotalDepositCollected = 100000,
            RoomParticipants = new List<RoomParticipant>
            {
                new() { UserId = hostId, DepositPaid = true, DepositAmount = 50000 },
                new() { UserId = playerId, DepositPaid = true, DepositAmount = 50000 }
            }
        };

        _matchRoomRepoMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        // THÊM MỚI: Giả lập Booking đang tồn tại và đã được Confirmed
        var booking = new Booking
        {
            BookingId = Guid.NewGuid(),
            RoomId = roomId,
            Status = BookingStatus.Confirmed
        };
        _bookingRepoMock.Setup(x => x.GetBookingByRoomAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Mock Wallets
        var hostWallet = new Wallet { WalletId = Guid.NewGuid(), UserId = hostId, Balance = 10000 };
        var playerWallet = new Wallet { WalletId = Guid.NewGuid(), UserId = playerId, Balance = 5000 };

        _walletRepoMock.Setup(x => x.GetByUserIdAsync(hostId, It.IsAny<CancellationToken>())).ReturnsAsync(hostWallet);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(playerId, It.IsAny<CancellationToken>())).ReturnsAsync(playerWallet);

        // Act
        var result = await _sut.Handle(new CancelMatchRoomCommand(roomId, "Bể kèo sớm"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PenaltyAmount.Should().Be(0); // Không bị phạt
        result.Value.RefundedAmount.Should().Be(100000); // Refund đủ 100k cho cả 2
        room.Status.Should().Be(RoomStatus.Cancelled);


        // Kiểm tra xem Booking đã bị hủy và hàm Update của Repo có được gọi không
        booking.Status.Should().Be(BookingStatus.Cancelled);
        _bookingRepoMock.Verify(x => x.Update(booking), Times.Once);

        // Verify Hangfire jobs are deleted
        _autoCloseMock.Verify(x => x.CancelAutoClose(It.IsAny<string>()), Times.Once);
        _lifecycleMock.Verify(x => x.CancelAllJobs(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        // Verify SignalR is called
        _hubServiceMock.Verify(x => x.NotifyRoomCancelledAsync(roomId, "Bể kèo sớm", It.IsAny<CancellationToken>()), Times.Once);

        // Verify Db Save
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InPenaltyZone_WithSufficientFunds_ChargesPenaltyAndCompensatesOwner()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();

        _userContextMock.Setup(x => x.UserId).Returns(hostId);

        // Giả lập trận đấu diễn ra sau 10 tiếng (Nằm trong Vùng Phạt 4h - 24h)
        var matchTime = DateTime.UtcNow.AddHours(10);

        var room = new MatchRoom
        {
            RoomId = roomId,
            HostId = hostId,
            Status = RoomStatus.Locked,
            MatchDate = matchTime.Date,
            StartTime = matchTime.TimeOfDay,
            TotalDepositCollected = 100000,
            FieldId = fieldId,
            RoomParticipants = new List<RoomParticipant>
            {
                new() { UserId = hostId, DepositPaid = true, DepositAmount = 50000 },
                new() { UserId = playerId, DepositPaid = true, DepositAmount = 50000 }
            }
        };

        _matchRoomRepoMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        // Mock Field & Venue để lấy OwnerId
        _fieldRepoMock.Setup(x => x.GetFieldWithVenueAsync(fieldId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Field { Venue = new Venue { OwnerId = ownerId } });

        // Mock Booking
        var booking = new Booking { BookingId = Guid.NewGuid(), RoomId = roomId, Status = BookingStatus.Confirmed };
        _bookingRepoMock.Setup(x => x.GetBookingByRoomAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        // Mock Wallets cho cả 3 người (Host, Player thường, và Venue Owner)
        var hostWallet = new Wallet { WalletId = Guid.NewGuid(), UserId = hostId, Balance = 10000 };
        var playerWallet = new Wallet { WalletId = Guid.NewGuid(), UserId = playerId, Balance = 5000 };
        var ownerWallet = new Wallet { WalletId = Guid.NewGuid(), UserId = ownerId, Balance = 0 };

        _walletRepoMock.Setup(x => x.GetByUserIdAsync(hostId, It.IsAny<CancellationToken>())).ReturnsAsync(hostWallet);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(playerId, It.IsAny<CancellationToken>())).ReturnsAsync(playerWallet);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(ownerId, It.IsAny<CancellationToken>())).ReturnsAsync(ownerWallet);

        // Act
        var result = await _sut.Handle(new CancelMatchRoomCommand(roomId, "Hủy trong vùng phạt"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // 1. Kiểm tra Response trả về có đúng số liệu không
        result.Value.PenaltyAmount.Should().Be(25000); // Phạt 25% của 100k
        result.Value.RefundedAmount.Should().Be(100000); // 2 người đều được Refund đủ

        // 2. Kiểm tra Số dư Ví cuối cùng (Khớp hoàn toàn với bài toán kế toán)
        hostWallet.Balance.Should().Be(35000); // 10k gốc + 50k refund cọc - 25k phạt = 35k
        ownerWallet.Balance.Should().Be(25000); // 0 gốc + 25k đền bù = 25k

        // 3. Kiểm tra DB có tạo record WalletTransaction lưu vết dòng tiền này không
        // Xác nhận Host bị ghi log Trừ tiền phạt
        _walletTxRepoMock.Verify(x => x.AddAsync(It.Is<WalletTransaction>(tx =>
            tx.WalletId == hostWallet.WalletId &&
            tx.TransactionType == TransactionType.Penalty &&
            tx.Amount == -25000)), Times.Once);

        // Xác nhận Chủ sân được ghi log Nhận tiền đền bù
        _walletTxRepoMock.Verify(x => x.AddAsync(It.Is<WalletTransaction>(tx =>
            tx.WalletId == ownerWallet.WalletId &&
            tx.TransactionType == TransactionType.Compensation &&
            tx.Amount == 25000)), Times.Once);

        // 4. Kiểm tra trạng thái râu ria
        room.Status.Should().Be(RoomStatus.Cancelled);
        booking.Status.Should().Be(BookingStatus.Cancelled);
    }
}