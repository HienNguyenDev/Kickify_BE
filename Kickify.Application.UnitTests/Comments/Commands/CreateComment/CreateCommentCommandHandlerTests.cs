using FluentAssertions;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Comments.Commands.CreateComment;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;
using Kickify.Domain.Event;
using MediatR;
using Moq;

namespace Kickify.Application.UnitTests.Comments.Commands.CreateComment;

public class CreateCommentCommandHandlerTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock = new();
    private readonly Mock<IPostRepository> _postRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly CreateCommentCommandHandler _sut;

    public CreateCommentCommandHandlerTests()
    {
        _sut = new CreateCommentCommandHandler(
            _commentRepositoryMock.Object,
            _postRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object,
            _publisherMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ReturnsPostNotFound_UTCID101()
    {
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);
        _postRepositoryMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync((Post?)null);

        var command = new CreateCommentCommand { PostId = postId, Content = "comment" };
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(CommentErrors.PostNotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenParentCommentNotFound_ReturnsParentNotFound_UTCID102()
    {
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);
        _postRepositoryMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(new Post { PostId = postId, UserId = userId });
        _commentRepositoryMock.Setup(x => x.GetByIdAsync(parentId)).ReturnsAsync((Comment?)null);

        var command = new CreateCommentCommand { PostId = postId, ParentCommentId = parentId, Content = "reply" };
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(CommentErrors.ParentCommentNotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenTopLevelComment_CreatesAndIncrementsTotalComments_UTCID103()
    {
        var actorId = Guid.NewGuid();
        var postOwnerId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var post = new Post { PostId = postId, UserId = postOwnerId, TotalComments = 2 };

        _userContextMock.SetupGet(x => x.UserId).Returns(actorId);
        _postRepositoryMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(post);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(actorId))
            .ReturnsAsync(new User { UserId = actorId, Email = "actor@kickify.dev", FullName = "Actor" });

        var command = new CreateCommentCommand { PostId = postId, Content = "new comment" };
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        post.TotalComments.Should().Be(3);
        _commentRepositoryMock.Verify(x => x.AddAsync(It.Is<Comment>(c =>
            c.PostId == postId &&
            c.UserId == actorId &&
            c.ParentCommentId == null)), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<PostCommentCreatedDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenReplyComment_IncrementsParentTotalReplies_UTCID104()
    {
        var actorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var post = new Post { PostId = postId, UserId = Guid.NewGuid(), TotalComments = 0 };
        var parent = new Comment { CommentId = parentId, UserId = Guid.NewGuid(), PostId = postId, TotalReplies = 1, IsActive = true };

        _userContextMock.SetupGet(x => x.UserId).Returns(actorId);
        _postRepositoryMock.Setup(x => x.GetByIdAsync(postId)).ReturnsAsync(post);
        _commentRepositoryMock.Setup(x => x.GetByIdAsync(parentId)).ReturnsAsync(parent);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(actorId))
            .ReturnsAsync(new User { UserId = actorId, Email = "actor@kickify.dev", FullName = "Actor" });

        var command = new CreateCommentCommand { PostId = postId, ParentCommentId = parentId, Content = "reply comment" };
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        parent.TotalReplies.Should().Be(2);
        _commentRepositoryMock.Verify(x => x.Update(parent), Times.Once);
    }
}
