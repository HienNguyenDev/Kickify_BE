using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Users.Commands.UpdateFcmToken;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Users.Commands.UpdateFcmToken;

public class UpdateFcmTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly UpdateFcmTokenCommandHandler _sut;

    public UpdateFcmTokenCommandHandlerTests()
    {
        _sut = new UpdateFcmTokenCommandHandler(
            _userRepositoryMock.Object,
            _userContextMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new UpdateFcmTokenCommand
        {
            FcmToken = "new-token"
        };

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
    public async Task Handle_WhenUserExists_UpdatesFcmTokenAndReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new UpdateFcmTokenCommand
        {
            FcmToken = "new-token"
        };

        var user = new User
        {
            UserId = userId,
            FcmToken = "old-token"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Success.Should().BeTrue();

        user.FcmToken.Should().Be(command.FcmToken);

        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

