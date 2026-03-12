using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Auth.Commands.ChangePassword;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly ChangePasswordCommandHandler _sut;

    public ChangePasswordCommandHandlerTests()
    {
        _sut = new ChangePasswordCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundByEmailError()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            Email = "notfound@example.com",
            CurrentPassword = "AnyPassword",
            NewPassword = "NewPassword123!"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.NotFoundByEmail.Code);

        _userRepositoryMock.Verify(
            x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()),
            Times.Once);

        _passwordHasherMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordIsWrong_ReturnsWrongPasswordError()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            Email = "user@example.com",
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword123!"
        };

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = "hashed-password"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(command.CurrentPassword, user.PasswordHash!))
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.WrongPassword.Code);

        _userRepositoryMock.Verify(
            x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()),
            Times.Once);

        _passwordHasherMock.Verify(
            x => x.Verify(command.CurrentPassword, user.PasswordHash!),
            Times.Once);

        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenRequestValid_UpdatesPasswordAndReturnsSuccess()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            Email = "user@example.com",
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = "old-hash"
        };

        const string newHashedPassword = "new-hash";

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(command.CurrentPassword, user.PasswordHash!))
            .Returns(true);

        _passwordHasherMock
            .Setup(x => x.Hash(command.NewPassword))
            .Returns(newHashedPassword);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(user.UserId);

        user.PasswordHash.Should().Be(newHashedPassword);

        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

