using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.MatchRooms.Commands.LeaveRoom;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Kickify.Application.UnitTests.MatchRooms.Commands.LeaveRoom;

public class LeaveRoomCommandHandlerTests
{
    private readonly Mock<IMatchRoomRepository> _matchRoomRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRoomParticipantRepository> _participantRepoMock = new();
    private readonly Mock<IChatMessageRepository> _chatRepoMock = new();
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly Mock<IWalletTransactionRepository> _walletTxRepoMock = new();
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IRoomAutoCloseService> _autoCloseMock = new();
    private readonly Mock<IMatchRoomHubService> _hubMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<ILogger<LeaveRoomCommandHandler>> _loggerMock = new();

    private readonly LeaveRoomCommandHandler _sut;

    public LeaveRoomCommandHandlerTests()
    {
        _sut = new LeaveRoomCommandHandler(
            _matchRoomRepoMock.Object, _userRepoMock.Object, _participantRepoMock.Object,
            _chatRepoMock.Object, _walletRepoMock.Object, _walletTxRepoMock.Object,
            _bookingRepoMock.Object, _autoCloseMock.Object, _hubMock.Object,
            _unitOfWorkMock.Object, _userContextMock.Object, _publisherMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRoomIsLocked_ReturnsLeaveNotAllowedError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });

        var room = new MatchRoom { RoomId = roomId, Status = RoomStatus.Locked }; // Trạng thái Locked
        _matchRoomRepoMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Act
        var result = await _sut.Handle(new LeaveRoomCommand(roomId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("MatchRoom.LeaveNotAllowed");
    }

    [Fact]
    public async Task Handle_RegularPlayerLeaves_OpenRoom_RefundsMoneyAndRemovesParticipant()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(playerId);
        _userRepoMock.Setup(x => x.GetByIdAsync(playerId)).ReturnsAsync(new User { UserId = playerId });

        var participant = new RoomParticipant { UserId = playerId, DepositPaid = true, DepositAmount = 50000 };
        var room = new MatchRoom
        {
            RoomId = roomId,
            HostId = hostId,
            Status = RoomStatus.Open,
            FilledSlots = 2,
            TotalDepositCollected = 100000,
            RoomParticipants = new List<RoomParticipant> { new() { UserId = hostId }, participant }
        };

        _matchRoomRepoMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var playerWallet = new Wallet { WalletId = Guid.NewGuid(), UserId = playerId, Balance = 10000 };
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(playerId, It.IsAny<CancellationToken>())).ReturnsAsync(playerWallet);

        // Act
        var result = await _sut.Handle(new LeaveRoomCommand(roomId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Kiểm tra tiền đã được hoàn
        playerWallet.Balance.Should().Be(60000); // 10k cũ + 50k refund
        room.TotalDepositCollected.Should().Be(50000); // Bị trừ đi 50k
        
        // Kiểm tra Transaction Refund được tạo
        _walletTxRepoMock.Verify(x => x.AddAsync(It.Is<WalletTransaction>(tx => 
            tx.TransactionType == TransactionType.Refund && tx.Amount == 50000)), Times.Once);

        // Kiểm tra data cập nhật
        room.FilledSlots.Should().Be(1);
        _participantRepoMock.Verify(x => x.Remove(participant), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_HostLeavesAlone_DeletesRoomAndCancelsBooking()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(hostId);
        _userRepoMock.Setup(x => x.GetByIdAsync(hostId)).ReturnsAsync(new User { UserId = hostId });

        var participant = new RoomParticipant { UserId = hostId, DepositPaid = true, DepositAmount = 50000 };
        var room = new MatchRoom
        {
            RoomId = roomId, HostId = hostId, Status = RoomStatus.Open, FilledSlots = 1, AutoCloseJobId = "Job123",
            RoomParticipants = new List<RoomParticipant> { participant }
        };

        var booking = new Booking { BookingId = Guid.NewGuid(), RoomId = roomId, Status = BookingStatus.Pending };

        _matchRoomRepoMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync(room);
        _bookingRepoMock.Setup(x => x.GetBookingByRoomAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        _walletRepoMock.Setup(x => x.GetByUserIdAsync(hostId, It.IsAny<CancellationToken>())).ReturnsAsync(new Wallet());

        // Act
        var result = await _sut.Handle(new LeaveRoomCommand(roomId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Room bị xóa, AutoClose job bị hủy
        _matchRoomRepoMock.Verify(x => x.Remove(room), Times.Once);
        _autoCloseMock.Verify(x => x.CancelAutoClose("Job123"), Times.Once);

        // Booking bị cancel
        booking.Status.Should().Be(BookingStatus.Cancelled);
        _bookingRepoMock.Verify(x => x.Update(booking), Times.Once);
    }

    [Fact]
    public async Task Handle_HostLeavesWithOthersRemaining_ReassignsHostAndResetsPrivacy()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var newHostId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(hostId);
        _userRepoMock.Setup(x => x.GetByIdAsync(hostId)).ReturnsAsync(new User { UserId = hostId });

        var hostParticipant = new RoomParticipant { UserId = hostId, DepositPaid = false };
        var otherParticipant = new RoomParticipant { UserId = newHostId };
        
        var room = new MatchRoom
        {
            RoomId = roomId, HostId = hostId, Status = RoomStatus.Open, FilledSlots = 2,
            Visibility = Visibility.Private, RoomPassword = "secret123",
            RoomParticipants = new List<RoomParticipant> { hostParticipant, otherParticipant }
        };

        _matchRoomRepoMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        // Act
        var result = await _sut.Handle(new LeaveRoomCommand(roomId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Host mới được gán
        room.HostId.Should().Be(newHostId);
        
        // Privacy được reset
        room.Visibility.Should().Be(Visibility.Public);
        room.RoomPassword.Should().BeNull();

        // System message được tạo ra
        _chatRepoMock.Verify(x => x.AddAsync(It.Is<ChatMessage>(m => 
            m.MessageType == MessageType.System && m.RoomId == roomId)), Times.Once);
            
        // Gửi SignalR thông báo Privacy thay đổi
        _hubMock.Verify(x => x.NotifyRoomPrivacyUpdatedAsync(roomId, Visibility.Public.ToString(), false, It.IsAny<CancellationToken>()), Times.Once);
    }
}