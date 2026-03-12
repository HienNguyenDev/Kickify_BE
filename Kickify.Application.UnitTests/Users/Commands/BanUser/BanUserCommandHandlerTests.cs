using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Users.Commands.BanUser;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Users.Commands.BanUser;

public class BanUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly BanUserCommandHandler _sut;

    public BanUserCommandHandlerTests()
    {
        _sut = new BanUserCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new BanUserCommand(userId, BanDuration.SevenDays);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.NotFound(userId).Code);

        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_ReturnsCannotBanAdmin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new BanUserCommand(userId, BanDuration.SevenDays);

        var user = new User
        {
            UserId = userId,
            Role = UserRole.Admin,
            Email = "admin@example.com"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(BanErrors.CannotBanAdmin.Code);

        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenDurationIsPermanent_SetsInactiveAndNoEndDate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new BanUserCommand(userId, BanDuration.Permanent);

        var user = new User
        {
            UserId = userId,
            Role = UserRole.Player,
            Email = "user@example.com",
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(userId);
        result.Value.BanDuration.Should().Be("Permanent");
        result.Value.IsActive.Should().BeFalse();
        result.Value.BannedUntil.Should().BeNull();

        user.IsActive.Should().BeFalse();
        user.BannedUntil.Should().BeNull();

        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDurationIsSevenDays_SetsBannedUntilAndInactive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new BanUserCommand(userId, BanDuration.SevenDays);

        var user = new User
        {
            UserId = userId,
            Role = UserRole.Player,
            Email = "user@example.com",
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var before = DateTime.UtcNow;
        var result = await _sut.Handle(command, CancellationToken.None);
        var after = DateTime.UtcNow;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(userId);
        result.Value.IsActive.Should().BeFalse();
        result.Value.BanDuration.Should().Contain("day");

        user.IsActive.Should().BeFalse();
        user.BannedUntil.Should().NotBeNull();
        user.BannedUntil!.Value.Should().BeOnOrAfter(before.AddDays(7).AddMinutes(-1));
        user.BannedUntil!.Value.Should().BeOnOrBefore(after.AddDays(7).AddMinutes(1));

        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

