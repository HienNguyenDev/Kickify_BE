using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.OTP;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Auth.Commands.RegisterPlayer;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Auth.Commands.RegisterPlayer;

public class RegisterPlayerCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IWalletRepository> _walletRepositoryMock = new();
    private readonly Mock<IPlayerProfileRepository> _playerProfileRepositoryMock = new();
    private readonly Mock<INotificationPreferenceRepository> _notificationPreferenceRepositoryMock = new();
    private readonly Mock<IAuthenticationServices> _authenticationServicesMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IOtpGenerator> _otpGeneratorMock = new();
    private readonly Mock<IRedisOtpStore> _otpStoreMock = new();

    private readonly RegisterPlayerCommandHandler _sut;

    public RegisterPlayerCommandHandlerTests()
    {
        _sut = new RegisterPlayerCommandHandler(
            _userRepositoryMock.Object,
            _walletRepositoryMock.Object,
            _playerProfileRepositoryMock.Object,
            _notificationPreferenceRepositoryMock.Object,
            _authenticationServicesMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object,
            _otpGeneratorMock.Object,
            _otpStoreMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterPlayerCommand
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FullName = "Existing User"
        };

        var existingUser = new User
        {
            UserId = Guid.NewGuid(),
            Email = command.Email,
            DeletedAt = null
        };

        _userRepositoryMock
            .Setup(x => x.GetUserByEmailIgnoreFilterAsync(command.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.EmailAlreadyExists.Code);

        _authenticationServicesMock.VerifyNoOtherCalls();
        _walletRepositoryMock.VerifyNoOtherCalls();
        _playerProfileRepositoryMock.VerifyNoOtherCalls();
        _notificationPreferenceRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenRequestValid_CreatesUserWalletProfileNotificationPreferenceAndOtp()
    {
        // Arrange
        var command = new RegisterPlayerCommand
        {
            Email = "new@example.com",
            Password = "Password123!",
            FullName = "New Player"
        };

        _userRepositoryMock
            .Setup(x => x.GetUserByEmailIgnoreFilterAsync(command.Email))
            .ReturnsAsync((User?)null);

        const string identityId = "identity-123";
        const string hashedPassword = "hashed-password";
        const string otpValue = "123456";

        _authenticationServicesMock
            .Setup(x => x.RegisterAsync(command.Email, command.Password))
            .ReturnsAsync(identityId);

        _passwordHasherMock
            .Setup(x => x.Hash(command.Password))
            .Returns(hashedPassword);

        _otpGeneratorMock
            .Setup(x => x.Generate6Digits())
            .Returns(otpValue);

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be(command.Email);
        capturedUser.FullName.Should().Be(command.FullName);
        capturedUser.PasswordHash.Should().Be(hashedPassword);
        capturedUser.Role.Should().Be(UserRole.Player);
        capturedUser.IdentityId.Should().Be(identityId);
        capturedUser.IsActive.Should().BeTrue();
        capturedUser.IsEmailVerified.Should().BeFalse();

        _walletRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Wallet>(w => w.UserId == capturedUser.UserId && w.WalletType == WalletType.Player)),
            Times.Once);

        _playerProfileRepositoryMock.Verify(
            x => x.AddAsync(It.Is<PlayerProfile>(p => p.UserId == capturedUser.UserId)),
            Times.Once);

        _notificationPreferenceRepositoryMock.Verify(
            x => x.AddAsync(It.Is<NotificationPreference>(n => n.UserId == capturedUser.UserId)),
            Times.Once);

        _otpStoreMock.Verify(
            x => x.StoreAsync(capturedUser.UserId, otpValue, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

