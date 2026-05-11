using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Venues.Commands.SubmitVenueVerification;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.Venues.Commands.SubmitVenueVerification;

public class SubmitVenueVerificationCommandHandlerTests
{
    private readonly Mock<IVenueRepository> _venueRepositoryMock = new();
    private readonly Mock<IVenuePhotoRepository> _venuePhotoRepositoryMock = new();
    private readonly Mock<IVenueEvidenceRepository> _venueEvidenceRepositoryMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly SubmitVenueVerificationCommandHandler _sut;

    public SubmitVenueVerificationCommandHandlerTests()
    {
        _sut = new SubmitVenueVerificationCommandHandler(
            _venueRepositoryMock.Object,
            _venuePhotoRepositoryMock.Object,
            _venueEvidenceRepositoryMock.Object,
            _userContextMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenVenueDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var venueId = Guid.NewGuid();
        var command = new SubmitVenueVerificationCommand(venueId);

        _venueRepositoryMock
            .Setup(x => x.GetVenueForUpdateAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Venue?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(VenueErrors.NotFound(venueId).Code);

        _venueRepositoryMock.Verify(
            x => x.GetVenueForUpdateAsync(venueId, It.IsAny<CancellationToken>()),
            Times.Once);

        _venuePhotoRepositoryMock.VerifyNoOtherCalls();
        _venueEvidenceRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenUserIsNotOwner_ReturnsUnauthorizedFailure()
    {
        // Arrange
        var venueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var command = new SubmitVenueVerificationCommand(venueId);
        var venue = new Venue
        {
            VenueId = venueId,
            OwnerId = ownerId,
            Status = VenueStatus.Draft
        };

        _venueRepositoryMock
            .Setup(x => x.GetVenueForUpdateAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);

        _userContextMock.SetupGet(x => x.UserId).Returns(otherUserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(VenueErrors.Unauthorized.Code);

        _venueRepositoryMock.Verify(
            x => x.GetVenueForUpdateAsync(venueId, It.IsAny<CancellationToken>()),
            Times.Once);

        _venuePhotoRepositoryMock.VerifyNoOtherCalls();
        _venueEvidenceRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(VenueStatus.PendingVerification)]
    [InlineData(VenueStatus.Approved)]
    [InlineData(VenueStatus.Suspended)]
    public async Task Handle_WhenVenueStatusIsInvalid_ReturnsInvalidVerificationStatusFailure(VenueStatus status)
    {
        // Arrange
        var venueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var command = new SubmitVenueVerificationCommand(venueId);
        var venue = new Venue
        {
            VenueId = venueId,
            OwnerId = ownerId,
            Status = status
        };

        _venueRepositoryMock
            .Setup(x => x.GetVenueForUpdateAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);

        _userContextMock.SetupGet(x => x.UserId).Returns(ownerId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(VenueErrors.InvalidVerificationStatus.Code);

        _venueRepositoryMock.Verify(
            x => x.GetVenueForUpdateAsync(venueId, It.IsAny<CancellationToken>()),
            Times.Once);

        _venuePhotoRepositoryMock.VerifyNoOtherCalls();
        _venueEvidenceRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenNoPhotos_ReturnsInsufficientPhotosFailure()
    {
        // Arrange
        var venueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var command = new SubmitVenueVerificationCommand(venueId);
        var venue = new Venue
        {
            VenueId = venueId,
            OwnerId = ownerId,
            Status = VenueStatus.Draft
        };

        _venueRepositoryMock
            .Setup(x => x.GetVenueForUpdateAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);

        _userContextMock.SetupGet(x => x.UserId).Returns(ownerId);

        _venuePhotoRepositoryMock
            .Setup(x => x.GetPhotosByVenueIdAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<VenuePhoto>());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(VenueErrors.InsufficientPhotos.Code);

        _venueEvidenceRepositoryMock.VerifyNoOtherCalls();
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenNoEvidence_ReturnsInsufficientEvidencesFailure()
    {
        // Arrange
        var venueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var command = new SubmitVenueVerificationCommand(venueId);
        var venue = new Venue
        {
            VenueId = venueId,
            OwnerId = ownerId,
            Status = VenueStatus.Draft
        };

        _venueRepositoryMock
            .Setup(x => x.GetVenueForUpdateAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);

        _userContextMock.SetupGet(x => x.UserId).Returns(ownerId);

        _venuePhotoRepositoryMock
            .Setup(x => x.GetPhotosByVenueIdAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new VenuePhoto() });

        _venueEvidenceRepositoryMock
            .Setup(x => x.CountByVenueIdAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(VenueErrors.InsufficientEvidences.Code);

        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_UpdatesVenueAndReturnsSuccess()
    {
        // Arrange
        var venueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var command = new SubmitVenueVerificationCommand(venueId);
        var venue = new Venue
        {
            VenueId = venueId,
            OwnerId = ownerId,
            Status = VenueStatus.Draft,
            AdminNotes = "Some note"
        };

        _venueRepositoryMock
            .Setup(x => x.GetVenueForUpdateAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);

        _userContextMock.SetupGet(x => x.UserId).Returns(ownerId);

        _venuePhotoRepositoryMock
            .Setup(x => x.GetPhotosByVenueIdAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new VenuePhoto() });

        _venueEvidenceRepositoryMock
            .Setup(x => x.CountByVenueIdAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.VenueId.Should().Be(command.VenueId);
        result.Value.Status.Should().Be(VenueStatus.PendingVerification.ToString());

        venue.Status.Should().Be(VenueStatus.PendingVerification);
        venue.AdminNotes.Should().BeNull();

        _venueRepositoryMock.Verify(x => x.Update(venue), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

