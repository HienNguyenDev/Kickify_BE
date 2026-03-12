using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.OTP;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Auth.Commands.ResendOtp;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Auth.Commands.ResendOtp;

public class ResendOtpCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRedisOtpStore> _otpStoreMock = new();
    private readonly Mock<IOtpGenerator> _otpGeneratorMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly ResendOtpCommandHandler _sut;

    public ResendOtpCommandHandlerTests()
    {
        _sut = new ResendOtpCommandHandler(
            _userRepositoryMock.Object,
            _otpStoreMock.Object,
            _otpGeneratorMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var command = new ResendOtpCommand
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

        _otpGeneratorMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyVerified_ReturnsUserAlreadyVerified()
    {
        // Arrange
        var command = new ResendOtpCommand
        {
            UserId = Guid.NewGuid()
        };

        var user = new User
        {
            UserId = command.UserId,
            Email = "user@example.com",
            IsEmailVerified = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.UserAlreadyVerified.Code);

        _otpGeneratorMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenRequestValid_GeneratesAndStoresOtp()
    {
        // Arrange
        var command = new ResendOtpCommand
        {
            UserId = Guid.NewGuid()
        };

        var user = new User
        {
            UserId = command.UserId,
            Email = "user@example.com",
            IsEmailVerified = false
        };

        const string otpValue = "999999";

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _otpGeneratorMock
            .Setup(x => x.Generate6Digits())
            .Returns(otpValue);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(user.UserId);

        _otpStoreMock.Verify(
            x => x.StoreAsync(user.UserId, otpValue, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

