using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.OTP;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Auth.Commands.RegisterVenueOwner;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Auth.Commands.RegisterVenueOwner;

public class RegisterVenueOwnerCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IWalletRepository> _walletRepositoryMock = new();
    private readonly Mock<IAuthenticationServices> _authenticationServicesMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IOtpGenerator> _otpGeneratorMock = new();
    private readonly Mock<IRedisOtpStore> _otpStoreMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly RegisterVenueOwnerCommandHandler _sut;

    public RegisterVenueOwnerCommandHandlerTests()
    {
        _sut = new RegisterVenueOwnerCommandHandler(
            _userRepositoryMock.Object,
            _walletRepositoryMock.Object,
            _authenticationServicesMock.Object,
            _passwordHasherMock.Object,
            _otpGeneratorMock.Object,
            _otpStoreMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterVenueOwnerCommand
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FullName = "Existing Owner"
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
        _otpStoreMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenRequestValid_CreatesVenueOwnerAndWalletAndOtp()
    {
        // Arrange
        var command = new RegisterVenueOwnerCommand
        {
            Email = "owner@example.com",
            Password = "Password123!",
            FullName = "New Owner"
        };

        _userRepositoryMock
            .Setup(x => x.GetUserByEmailIgnoreFilterAsync(command.Email))
            .ReturnsAsync((User?)null);

        const string identityId = "identity-venue-owner";
        const string hashedPassword = "hashed-password";
        const string otpValue = "654321";

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
        capturedUser.Role.Should().Be(UserRole.VenueOwner);
        capturedUser.IdentityId.Should().Be(identityId);

        _walletRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Wallet>(w => w.UserId == capturedUser.UserId && w.WalletType == WalletType.VenueOwner)),
            Times.Once);

        _otpStoreMock.Verify(
            x => x.StoreAsync(capturedUser.UserId, otpValue, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

