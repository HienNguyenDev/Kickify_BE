using System.IO;
using FluentAssertions;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.Posts.Commands.CreatePost;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Posts.Commands.CreatePost;

public class CreatePostCommandHandlerTests
{
    private readonly Mock<IStorageService> _storageServiceMock = new();
    private readonly Mock<IPostRepository> _postRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly CreatePostCommandHandler _sut;

    public CreatePostCommandHandlerTests()
    {
        _sut = new CreatePostCommandHandler(
            _storageServiceMock.Object,
            _postRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUploadPartiallyFails_ReturnsUploadFailed_UTCID84()
    {
        var userId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new CreatePostCommand
        {
            Content = "post with files",
            Files =
            [
                new FileUploadRequest(new MemoryStream([1, 2, 3]), "ok.jpg", "image/jpeg", 3),
                new FileUploadRequest(new MemoryStream([4, 5]), "fail.jpg", "image/jpeg", 2)
            ]
        };

        _storageServiceMock.Setup(x => x.UploadMultipleAsync(command.Files, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new UploadResult(true, "obj-ok", "https://cdn/ok.jpg", 3),
                new UploadResult(false, string.Empty, string.Empty, 0, "quota exceeded")
            ]);
        _storageServiceMock.Setup(x => x.DeleteAsync("obj-ok", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(PostErrors.UploadFailed("quota exceeded").Code);
        _storageServiceMock.Verify(x => x.DeleteAsync("obj-ok", It.IsAny<CancellationToken>()), Times.Once);
        _postRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Post>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTextOnlyInput_PersistsPostWithoutMedia_UTCID85()
    {
        var userId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new CreatePostCommand { Content = "hello world" };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Success.Should().BeTrue();
        result.Value.Media.Should().NotBeNull();
        result.Value.Media!.Should().BeEmpty();

        _postRepositoryMock.Verify(x => x.AddAsync(It.Is<Post>(p =>
            p.UserId == userId &&
            p.Content == "hello world" &&
            p.TotalMedia == 0)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMediaUploadSucceeds_ReturnsMediaDtos_UTCID86()
    {
        var userId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var files = new List<FileUploadRequest>
        {
            new(new MemoryStream([1, 2]), "img.png", "image/png", 2),
            new(new MemoryStream([3, 4]), "video.mp4", "video/mp4", 2)
        };
        var command = new CreatePostCommand { Content = "media", Files = files };

        _storageServiceMock.Setup(x => x.UploadMultipleAsync(files, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new UploadResult(true, "obj-img", "https://cdn/img.png", 2),
                new UploadResult(true, "obj-video", "https://cdn/video.mp4", 2)
            ]);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Media.Should().HaveCount(2);
        result.Value.Media!.Select(m => m.Type).Should().BeEquivalentTo(["Image", "Video"]);
    }
}
