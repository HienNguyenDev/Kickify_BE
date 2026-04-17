using FluentAssertions;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Friendships.Commands.SendFriendRequest;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Kickify.Domain.Event;
using MediatR;
using Moq;

namespace Kickify.Application.UnitTests.Friendships.Commands.SendFriendRequest;

public class SendFriendRequestCommandHandlerTests
{
    private readonly Mock<IFriendshipRepository> _friendshipRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly SendFriendRequestCommandHandler _sut;

    public SendFriendRequestCommandHandlerTests()
    {
        _sut = new SendFriendRequestCommandHandler(
            _friendshipRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object,
            _publisherMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAddSelf_ReturnsCannotAddSelf_UTCID54()
    {
        var userId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new SendFriendRequestCommand { AddresseeId = userId };
        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(FriendshipErrors.CannotAddSelf.Code);
    }

    [Fact]
    public async Task Handle_WhenAlreadyFriends_ReturnsAlreadyFriends_UTCID57()
    {
        var requesterId = Guid.NewGuid();
        var addresseeId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(requesterId);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(addresseeId))
            .ReturnsAsync(new User { UserId = addresseeId, Email = "target@kickify.dev" });
        _userRepositoryMock.Setup(x => x.GetByIdAsync(requesterId))
            .ReturnsAsync(new User { UserId = requesterId, Email = "me@kickify.dev" });
        _friendshipRepositoryMock.Setup(x => x.GetFriendshipIncludeDeletedAsync(requesterId, addresseeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Friendship
            {
                FriendshipId = Guid.NewGuid(),
                RequesterId = requesterId,
                AddresseeId = addresseeId,
                Status = FriendshipStatus.Accepted
            });

        var result = await _sut.Handle(new SendFriendRequestCommand { AddresseeId = addresseeId }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(FriendshipErrors.AlreadyFriends.Code);
    }

    [Fact]
    public async Task Handle_WhenSoftDeletedFriendshipExists_ReopensAndPublishesEvent_UTCID59()
    {
        var requesterId = Guid.NewGuid();
        var addresseeId = Guid.NewGuid();
        var existing = new Friendship
        {
            FriendshipId = Guid.NewGuid(),
            RequesterId = addresseeId,
            AddresseeId = requesterId,
            Status = FriendshipStatus.Accepted,
            DeletedAt = DateTime.UtcNow
        };

        _userContextMock.SetupGet(x => x.UserId).Returns(requesterId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(addresseeId))
            .ReturnsAsync(new User { UserId = addresseeId, Email = "target@kickify.dev", FullName = "Target User" });
        _userRepositoryMock.Setup(x => x.GetByIdAsync(requesterId))
            .ReturnsAsync(new User { UserId = requesterId, Email = "me@kickify.dev", FullName = "Current User" });
        _friendshipRepositoryMock.Setup(x => x.GetFriendshipIncludeDeletedAsync(requesterId, addresseeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.Handle(new SendFriendRequestCommand { AddresseeId = addresseeId }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existing.DeletedAt.Should().BeNull();
        existing.Status.Should().Be(FriendshipStatus.Pending);
        existing.RequesterId.Should().Be(requesterId);
        existing.AddresseeId.Should().Be(addresseeId);

        _friendshipRepositoryMock.Verify(x => x.Update(existing), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<FriendRequestSentDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
