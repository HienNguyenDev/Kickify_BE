using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.Users.Commands.UploadUserAvatar;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Users.Commands.UploadUserAvatar;

public class UploadUserAvatarCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();

    private readonly UploadUserAvatarCommandHandler _sut;

    public UploadUserAvatarCommandHandlerTests()
    {
        _sut = new UploadUserAvatarCommandHandler(
            _userRepositoryMock.Object,
            _storageServiceMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new UploadUserAvatarCommand
        {
            File = new FileUploadRequest(new MemoryStream(), "avatar.png", "image/png", 10)
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
    public async Task Handle_WhenUploadFails_ReturnsAvatarUploadFailed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var user = new User
        {
            UserId = userId,
            Email = "user@example.com",
            AvatarUrl = null
        };

        var command = new UploadUserAvatarCommand
        {
            File = new FileUploadRequest(new MemoryStream(), "avatar.png", "image/png", 10)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _storageServiceMock
            .Setup(x => x.UploadAsync(
                command.File.Stream,
                command.File.FileName,
                command.File.ContentType,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UploadResult(false, string.Empty, string.Empty, 0, "error"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.AvatarUploadFailed("error").Code);
    }

    [Fact]
    public async Task Handle_WhenUserHasExistingAvatar_DeletesOldObject()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var user = new User
        {
            UserId = userId,
            Email = "user@example.com",
            AvatarUrl = "https://bucket.storage.com/avatars/old-avatar.png"
        };

        var command = new UploadUserAvatarCommand
        {
            File = new FileUploadRequest(new MemoryStream(), "avatar.png", "image/png", 10)
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _storageServiceMock
            .Setup(x => x.UploadAsync(
                command.File.Stream,
                command.File.FileName,
                command.File.ContentType,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UploadResult(true, "avatars/new-avatar.png", "https://bucket/avatars/new-avatar.png", 10));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(user.UserId);
        result.Value.AvatarUrl.Should().Be(user.AvatarUrl);

        _storageServiceMock.Verify(
            x => x.DeleteAsync(It.Is<string>(s => s.Contains("old-avatar.png")), It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

