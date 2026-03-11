using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Users.Commands.CreateUser;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Users.Commands.CreateUser;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPlayerProfileRepository> _playerProfileRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly CreateUserCommandHandler _sut;

    public CreateUserCommandHandlerTests()
    {
        _sut = new CreateUserCommandHandler(
            _userRepositoryMock.Object,
            _playerProfileRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEmailExists_ReturnsEmailAlreadyExists()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FullName = "Existing User",
            Role = UserRole.Player
        };

        _userRepositoryMock
            .Setup(x => x.IsEmailExistsAsync(command.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.EmailAlreadyExists.Code);

        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenRequestValid_CreatesUserAndPlayerProfile()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "new@example.com",
            Password = "Password123!",
            FullName = "New User",
            Phone = "123456789",
            Bio = "Bio",
            DateOfBirth = new DateTime(2000, 1, 1),
            Gender = Gender.Male,
            Role = UserRole.Player
        };

        const string hashedPassword = "hashed-password";

        _userRepositoryMock
            .Setup(x => x.IsEmailExistsAsync(command.Email))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(x => x.Hash(command.Password))
            .Returns(hashedPassword);

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .Returns(Task.CompletedTask);

        PlayerProfile? capturedProfile = null;
        _playerProfileRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<PlayerProfile>()))
            .Callback<PlayerProfile>(p => capturedProfile = p)
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
        capturedUser.Role.Should().Be(command.Role);
        capturedUser.IsActive.Should().BeTrue();
        capturedUser.IsEmailVerified.Should().BeFalse();

        capturedProfile.Should().NotBeNull();
        capturedProfile!.UserId.Should().Be(capturedUser.UserId);
        capturedProfile.CurrentElo.Should().Be(1000);
        capturedProfile.TrustScore.Should().Be(100);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

