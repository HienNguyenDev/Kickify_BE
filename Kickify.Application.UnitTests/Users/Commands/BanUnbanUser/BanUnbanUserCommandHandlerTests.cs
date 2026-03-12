using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Users.Commands.BanUnbanUser;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Users.Commands.BanUnbanUser;

public class BanUnbanUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly BanUnbanUserCommandHandler _sut;

    public BanUnbanUserCommandHandlerTests()
    {
        _sut = new BanUnbanUserCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new BanUnbanUserCommand(userId, IsActive: false);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.NotFound(userId).Code);
    }

    [Fact]
    public async Task Handle_WhenBanAlreadyBannedUser_ReturnsAlreadyBanned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new BanUnbanUserCommand(userId, IsActive: false);

        var user = new User
        {
            UserId = userId,
            IsActive = false
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.AlreadyBanned.Code);
    }

    [Fact]
    public async Task Handle_WhenUnbanNotBannedUser_ReturnsNotBanned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new BanUnbanUserCommand(userId, IsActive: true);

        var user = new User
        {
            UserId = userId,
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.NotBanned.Code);
    }

    [Fact]
    public async Task Handle_WhenBanActiveUser_SetsInactive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new BanUnbanUserCommand(userId, IsActive: false);

        var user = new User
        {
            UserId = userId,
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
        result.Value.IsActive.Should().BeFalse();

        user.IsActive.Should().BeFalse();

        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUnbanBannedUser_ClearsBan()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new BanUnbanUserCommand(userId, IsActive: true);

        var user = new User
        {
            UserId = userId,
            IsActive = false,
            BannedUntil = DateTime.UtcNow.AddDays(1)
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
        result.Value.IsActive.Should().BeTrue();

        user.IsActive.Should().BeTrue();
        user.BannedUntil.Should().BeNull();

        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

