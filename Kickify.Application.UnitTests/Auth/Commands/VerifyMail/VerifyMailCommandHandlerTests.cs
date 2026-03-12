using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.OTP;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Auth.Commands.VerifyMail;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Auth.Commands.VerifyMail;

public class VerifyMailCommandHandlerTests
{
    private readonly Mock<IRedisOtpStore> _otpStoreMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly VerifyMailCommandHandler _sut;

    public VerifyMailCommandHandlerTests()
    {
        _sut = new VerifyMailCommandHandler(
            _otpStoreMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var command = new VerifyMailCommand
        {
            UserId = Guid.NewGuid(),
            Otp = "123456"
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

        _otpStoreMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenOtpExpired_ReturnsOtpExpired()
    {
        // Arrange
        var command = new VerifyMailCommand
        {
            UserId = Guid.NewGuid(),
            Otp = "123456"
        };

        var user = new User
        {
            UserId = command.UserId,
            Email = "user@example.com",
            IsEmailVerified = false
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _otpStoreMock
            .Setup(x => x.GetAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.OtpExpired.Code);

        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenOtpWrong_ReturnsWrongOtp()
    {
        // Arrange
        var command = new VerifyMailCommand
        {
            UserId = Guid.NewGuid(),
            Otp = "wrong-otp"
        };

        var user = new User
        {
            UserId = command.UserId,
            Email = "user@example.com",
            IsEmailVerified = false
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _otpStoreMock
            .Setup(x => x.GetAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("expected-otp");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.WrongOtp.Code);

        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenRequestValid_VerifiesEmailAndRemovesOtp()
    {
        // Arrange
        var command = new VerifyMailCommand
        {
            UserId = Guid.NewGuid(),
            Otp = "123456"
        };

        var user = new User
        {
            UserId = command.UserId,
            Email = "user@example.com",
            IsEmailVerified = false
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _otpStoreMock
            .Setup(x => x.GetAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command.Otp);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(user.UserId);

        user.IsEmailVerified.Should().BeTrue();

        _otpStoreMock.Verify(x => x.RemoveAsync(command.UserId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

