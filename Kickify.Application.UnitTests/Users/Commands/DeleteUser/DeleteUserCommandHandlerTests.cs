using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Users.Commands.DeleteUser;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Users.Commands.DeleteUser;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IAuthenticationServices> _authenticationServicesMock = new();

    private readonly DeleteUserCommandHandler _sut;

    public DeleteUserCommandHandlerTests()
    {
        _sut = new DeleteUserCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _authenticationServicesMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var command = new DeleteUserCommand
        {
            UserId = Guid.NewGuid()
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.NotFound(command.UserId).Code);

        _userRepositoryMock.Verify(x => x.GetByIdAsync(command.UserId), Times.Once);
        _userRepositoryMock.VerifyNoOtherCalls();
        _authenticationServicesMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenUserHasNoIdentityId_RemovesUserAndReturnsResponse()
    {
        // Arrange
        var command = new DeleteUserCommand
        {
            UserId = Guid.NewGuid()
        };

        var deletedAt = DateTime.UtcNow;

        var user = new User
        {
            UserId = command.UserId,
            Email = "user@example.com",
            IdentityId = null,
            DeletedAt = deletedAt
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(user.UserId);
        result.Value.DeletedAt.Should().Be(user.DeletedAt);

        _userRepositoryMock.Verify(x => x.Remove(user), Times.Once);
        _authenticationServicesMock.Verify(
            x => x.DeleteUserAsync(It.IsAny<string>()),
            Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasIdentityId_CallsAuthServiceDeleteUser()
    {
        // Arrange
        var command = new DeleteUserCommand
        {
            UserId = Guid.NewGuid()
        };

        var deletedAt = DateTime.UtcNow;

        var user = new User
        {
            UserId = command.UserId,
            Email = "user@example.com",
            IdentityId = "auth-123",
            DeletedAt = deletedAt
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(user.UserId);

        _userRepositoryMock.Verify(x => x.Remove(user), Times.Once);
        _authenticationServicesMock.Verify(
            x => x.DeleteUserAsync(user.IdentityId!),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

