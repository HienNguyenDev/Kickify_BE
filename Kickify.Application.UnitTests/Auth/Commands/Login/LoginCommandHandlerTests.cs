using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Auth.Commands.Login;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Auth.Commands.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        _sut = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtProviderMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundByEmail()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "notfound@example.com",
            Password = "Password123!"
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

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.VerifyNoOtherCalls();
        _refreshTokenRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenPasswordInvalid_ReturnsWrongPassword()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "user@example.com",
            Password = "WrongPassword"
        };

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = "hashed-password",
            IsActive = true,
            IsEmailVerified = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(command.Password, user.PasswordHash!))
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.WrongPassword.Code);

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(command.Password, user.PasswordHash!), Times.Once);
        _refreshTokenRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenUserInactiveAndBanNotExpired_ReturnsInactive()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = "hashed-password",
            IsActive = false,
            BannedUntil = DateTime.UtcNow.AddHours(1),
            IsEmailVerified = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(command.Password, user.PasswordHash!))
            .Returns(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.InActive.Code);

        user.IsActive.Should().BeFalse(); // vẫn bị ban

        _refreshTokenRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenUserNotVerified_ReturnsIsNotVerified()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = "hashed-password",
            IsActive = true,
            IsEmailVerified = false
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(command.Password, user.PasswordHash!))
            .Returns(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.IsNotVerified.Code);

        _refreshTokenRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenRequestValid_ReturnsTokensAndPersistsRefreshToken()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = "hashed-password",
            FullName = "Test User",
            Role = UserRole.Player,
            AvatarUrl = "avatar.png",
            IsActive = false,
            BannedUntil = DateTime.UtcNow.AddHours(-1), // đã hết hạn ban
            IsEmailVerified = true
        };

        var existingTokens = new List<RefreshToken>
        {
            new() { TokenId = Guid.NewGuid(), UserId = user.UserId, Token = "old", ExpiresAt = DateTime.UtcNow.AddDays(1) }
        };

        const string accessToken = "access-token";
        const string refreshTokenValue = "new-refresh-token";

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(command.Password, user.PasswordHash!))
            .Returns(true);

        _jwtProviderMock
            .Setup(x => x.GetForCredentialsAsync(command.Email))
            .ReturnsAsync(accessToken);

        _jwtProviderMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns(refreshTokenValue);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTokens);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(user.UserId);
        result.Value.Email.Should().Be(user.Email);
        result.Value.AccessToken.Should().Be(accessToken);
        result.Value.RefreshToken.Should().Be(refreshTokenValue);
        result.Value.IsActive.Should().BeTrue();

        user.IsActive.Should().BeTrue();
        user.BannedUntil.Should().BeNull();

        _refreshTokenRepositoryMock.Verify(x => x.RemoveRange(existingTokens), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

