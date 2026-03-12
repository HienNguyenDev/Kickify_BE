using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Users.Commands.UpdateUser;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Users.Commands.UpdateUser;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly UpdateUserCommandHandler _sut;

    public UpdateUserCommandHandlerTests()
    {
        _sut = new UpdateUserCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            FullName = "New Name"
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

        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenRequestValid_UpdatesUserAndReturnsResponse()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            FullName = "Updated Name",
            Phone = "123456789",
            AvatarUrl = "https://example.com/avatar.png",
            Bio = "Bio",
            DateOfBirth = new DateTime(2000, 1, 1),
            PreferredPositions = "ST",
            ShirtNumber = 10,
            PreferredFoot = "Right",
            Gender = Gender.Male
        };

        var user = new User
        {
            UserId = command.UserId,
            Email = "user@example.com"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(user.UserId);
        result.Value.Email.Should().Be(user.Email);
        result.Value.FullName.Should().Be(command.FullName);
        result.Value.Phone.Should().Be(command.Phone);
        result.Value.AvatarUrl.Should().Be(command.AvatarUrl);
        result.Value.Bio.Should().Be(command.Bio);
        result.Value.DateOfBirth.Should().Be(command.DateOfBirth);
        result.Value.Gender.Should().Be(command.Gender);
        result.Value.PreferredPositions.Should().Be(command.PreferredPositions);
        result.Value.ShirtNumber.Should().Be(command.ShirtNumber);
        result.Value.PreferredFoot.Should().Be(command.PreferredFoot);

        user.FullName.Should().Be(command.FullName);
        user.Phone.Should().Be(command.Phone);
        user.AvatarUrl.Should().Be(command.AvatarUrl);
        user.Bio.Should().Be(command.Bio);
        user.DateOfBirth.Should().Be(command.DateOfBirth);
        user.Gender.Should().Be(command.Gender);
        user.PreferredPositions.Should().Be(command.PreferredPositions);
        user.ShirtNumber.Should().Be(command.ShirtNumber);
        user.PreferredFoot.Should().Be(command.PreferredFoot);

        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

