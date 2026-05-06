using FluentAssertions;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Withdrawals.Commands.ProcessWithdrawal;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Withdrawals.Commands.ProcessWithdrawal;

public class ProcessWithdrawalCommandHandlerTests
{
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<IWalletWithdrawalRepository> _withdrawalRepositoryMock;
    private readonly Mock<IWalletTransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ProcessWithdrawalCommandHandler _handler;

    public ProcessWithdrawalCommandHandlerTests()
    {
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _withdrawalRepositoryMock = new Mock<IWalletWithdrawalRepository>();
        _transactionRepositoryMock = new Mock<IWalletTransactionRepository>();
        _userContextMock = new Mock<IUserContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new ProcessWithdrawalCommandHandler(
            _walletRepositoryMock.Object,
            _withdrawalRepositoryMock.Object,
            _transactionRepositoryMock.Object,
            _userContextMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ProcessingWithdrawal_WhenPendingStatus_ShouldReturnSuccess_UTCID36()
    {
        // Arrange
        var command = new ProcessWithdrawalCommand { WithdrawalId = Guid.NewGuid(), IsApproved = true, AdminNotes = null };
        var walletId = Guid.NewGuid();
        var withdrawal = new WalletWithdrawal { WithdrawalId = command.WithdrawalId, WalletId = walletId, Status = WithdrawalStatus.Pending, Amount = 100 };
        var wallet = new Wallet { WalletId = walletId, Balance = 150, WalletType = WalletType.Player };

        _withdrawalRepositoryMock.Setup(repo => repo.GetByIdAsync(command.WithdrawalId))
            .ReturnsAsync(withdrawal);
        _walletRepositoryMock.Setup(repo => repo.GetByIdAsync(walletId))
            .ReturnsAsync(wallet);
        _userContextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        withdrawal.Status.Should().Be(WithdrawalStatus.Completed);
        result.Value.WithdrawalFee.Should().Be(0);
        result.Value.PayoutAmount.Should().Be(100);
        _transactionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<WalletTransaction>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProcessingWithdrawal_WhenProcessingStatus_ShouldReturnSuccess_UTCID37()
    {
        // Arrange
        var command = new ProcessWithdrawalCommand { WithdrawalId = Guid.NewGuid(), IsApproved = true, AdminNotes = null };
        var walletId = Guid.NewGuid();
        var withdrawal = new WalletWithdrawal { WithdrawalId = command.WithdrawalId, WalletId = walletId, Status = WithdrawalStatus.Processing, Amount = 100 };
        var wallet = new Wallet { WalletId = walletId, Balance = 150, WalletType = WalletType.Player };

        _withdrawalRepositoryMock.Setup(repo => repo.GetByIdAsync(command.WithdrawalId))
            .ReturnsAsync(withdrawal);
        _walletRepositoryMock.Setup(repo => repo.GetByIdAsync(walletId))
            .ReturnsAsync(wallet);
        _userContextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        withdrawal.Status.Should().Be(WithdrawalStatus.Completed);
        result.Value.WithdrawalFee.Should().Be(0);
        result.Value.PayoutAmount.Should().Be(100);
        _transactionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<WalletTransaction>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ApprovingVenueOwnerWithdrawal_ShouldChargeFee_UTCID40()
    {
        // Arrange
        var command = new ProcessWithdrawalCommand { WithdrawalId = Guid.NewGuid(), IsApproved = true, AdminNotes = null };
        var walletId = Guid.NewGuid();
        var withdrawal = new WalletWithdrawal { WithdrawalId = command.WithdrawalId, WalletId = walletId, Status = WithdrawalStatus.Pending, Amount = 100_000m };
        var wallet = new Wallet { WalletId = walletId, Balance = 200_000m, WalletType = WalletType.VenueOwner };

        _withdrawalRepositoryMock.Setup(repo => repo.GetByIdAsync(command.WithdrawalId))
            .ReturnsAsync(withdrawal);
        _walletRepositoryMock.Setup(repo => repo.GetByIdAsync(walletId))
            .ReturnsAsync(wallet);
        _userContextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WithdrawalFee.Should().Be(1_000m);
        result.Value.PayoutAmount.Should().Be(99_000m);
        _transactionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<WalletTransaction>()), Times.Exactly(2));
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DecliningWithdrawal_ShouldUpdateStatusButNotDeductBalance_UTCID38()
    {
        // Arrange
        var command = new ProcessWithdrawalCommand { WithdrawalId = Guid.NewGuid(), IsApproved = false, AdminNotes = "Insufficient funds on bank side" };
        var walletId = Guid.NewGuid();
        var amount = 100m;
        var withdrawal = new WalletWithdrawal { WithdrawalId = command.WithdrawalId, WalletId = walletId, Status = WithdrawalStatus.Pending, Amount = amount };
        var wallet = new Wallet { WalletId = walletId, Balance = 150 };

        _withdrawalRepositoryMock.Setup(repo => repo.GetByIdAsync(command.WithdrawalId))
            .ReturnsAsync(withdrawal);
        _walletRepositoryMock.Setup(repo => repo.GetByIdAsync(walletId))
            .ReturnsAsync(wallet);
        _userContextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        withdrawal.Status.Should().Be(WithdrawalStatus.Rejected);
        wallet.Balance.Should().Be(150m);
        _transactionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<WalletTransaction>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProcessingWithdrawal_WhenInvalidStatus_ShouldReturnFailure_UTCID39()
    {
        // Arrange
        var command = new ProcessWithdrawalCommand { WithdrawalId = Guid.NewGuid(), IsApproved = true, AdminNotes = null };
        var withdrawal = new WalletWithdrawal { WithdrawalId = command.WithdrawalId, Status = WithdrawalStatus.Completed };

        _withdrawalRepositoryMock.Setup(repo => repo.GetByIdAsync(command.WithdrawalId))
            .ReturnsAsync(withdrawal);
        _userContextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(WalletErrors.WithdrawalNotPending);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
