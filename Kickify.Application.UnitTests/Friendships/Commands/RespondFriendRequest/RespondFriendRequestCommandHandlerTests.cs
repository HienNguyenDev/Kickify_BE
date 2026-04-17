using FluentAssertions;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Friendships.Commands.RespondFriendRequest;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Kickify.Domain.Event;
using MediatR;
using Moq;

namespace Kickify.Application.UnitTests.Friendships.Commands.RespondFriendRequest;

public class RespondFriendRequestCommandHandlerTests
{
    private readonly Mock<IFriendshipRepository> _friendshipRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly RespondFriendRequestCommandHandler _sut;

    public RespondFriendRequestCommandHandlerTests()
    {
        _sut = new RespondFriendRequestCommandHandler(
            _friendshipRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object,
            _publisherMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPendingRequestNotFound_ReturnsRequestNotFound_UTCID60()
    {
        var requesterId = Guid.NewGuid();
        var addresseeId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(addresseeId);
        _friendshipRepositoryMock.Setup(x => x.GetFriendshipAsync(requesterId, addresseeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Friendship?)null);

        var result = await _sut.Handle(new RespondFriendRequestCommand { RequesterId = requesterId, Accept = true }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(FriendshipErrors.RequestNotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenAcceptRequest_UpdatesAndPublishesEvent_UTCID62()
    {
        var requesterId = Guid.NewGuid();
        var addresseeId = Guid.NewGuid();
        var friendship = new Friendship
        {
            FriendshipId = Guid.NewGuid(),
            RequesterId = requesterId,
            AddresseeId = addresseeId,
            Status = FriendshipStatus.Pending
        };

        _userContextMock.SetupGet(x => x.UserId).Returns(addresseeId);
        _friendshipRepositoryMock.Setup(x => x.GetFriendshipAsync(requesterId, addresseeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendship);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(addresseeId))
            .ReturnsAsync(new User { UserId = addresseeId, Email = "receiver@kickify.dev", FullName = "Receiver" });

        var result = await _sut.Handle(new RespondFriendRequestCommand { RequesterId = requesterId, Accept = true }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        friendship.Status.Should().Be(FriendshipStatus.Accepted);
        friendship.RespondedAt.Should().NotBeNull();
        _friendshipRepositoryMock.Verify(x => x.Update(friendship), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<FriendRequestAcceptedDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDeclineRequest_RemovesFriendship_UTCID63()
    {
        var requesterId = Guid.NewGuid();
        var addresseeId = Guid.NewGuid();
        var friendship = new Friendship
        {
            FriendshipId = Guid.NewGuid(),
            RequesterId = requesterId,
            AddresseeId = addresseeId,
            Status = FriendshipStatus.Pending
        };

        _userContextMock.SetupGet(x => x.UserId).Returns(addresseeId);
        _friendshipRepositoryMock.Setup(x => x.GetFriendshipAsync(requesterId, addresseeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendship);

        var result = await _sut.Handle(new RespondFriendRequestCommand { RequesterId = requesterId, Accept = false }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Declined");
        _friendshipRepositoryMock.Verify(x => x.Remove(friendship), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<FriendRequestAcceptedDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
