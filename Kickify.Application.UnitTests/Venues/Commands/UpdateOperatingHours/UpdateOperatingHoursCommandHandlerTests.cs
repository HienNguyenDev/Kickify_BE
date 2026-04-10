using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Features.Venues.Commands.UpdateOperatingHours;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Moq;
using Xunit;

namespace Kickify.Application.UnitTests.Venues.Commands.UpdateOperatingHours;

public class UpdateOperatingHoursCommandHandlerTests
{
    private readonly Mock<IVenueRepository> _venueRepositoryMock = new();
    private readonly Mock<IVenueOperatingHourRepository> _operatingHourRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly UpdateOperatingHoursCommandHandler _sut;

    public UpdateOperatingHoursCommandHandlerTests()
    {
        _sut = new UpdateOperatingHoursCommandHandler(
            _venueRepositoryMock.Object,
            _operatingHourRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object);
    }

    // Covers UTCID09
    [Fact]
    public async Task Handle_VenueDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        var command = new UpdateOperatingHoursCommand { VenueId = Guid.NewGuid(), OperatingHours = new List<OperatingHourItemDto>() };
        
        _userContextMock.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _venueRepositoryMock.Setup(repo => repo.GetByIdAsync(command.VenueId))
            .ReturnsAsync((Venue?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(VenueErrors.NotFound(command.VenueId).Code);
    }

    // Covers UTCID10
    [Fact]
    public async Task Handle_UserIsNotOwner_ReturnsUnauthorizedError()
    {
        // Arrange
        var command = new UpdateOperatingHoursCommand { VenueId = Guid.NewGuid(), OperatingHours = new List<OperatingHourItemDto>() };
        var venue = new Venue { VenueId = command.VenueId, OwnerId = Guid.NewGuid() }; // Owner is different
        
        _userContextMock.Setup(u => u.UserId).Returns(Guid.NewGuid()); // Caller differs from Owner
        _venueRepositoryMock.Setup(repo => repo.GetByIdAsync(command.VenueId))
            .ReturnsAsync(venue);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(VenueErrors.Unauthorized.Code);
    }

    // Covers UTCID11
    [Fact]
    public async Task Handle_NullOperatingTimeInput_SkipsUpdatesRemainsUntouched()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new UpdateOperatingHoursCommand 
        { 
            VenueId = Guid.NewGuid(), 
            OperatingHours = new List<OperatingHourItemDto>
            {
                new OperatingHourItemDto((int)DayOfWeek.Monday, null, null)
            }
        };
        
        var venue = new Venue { VenueId = command.VenueId, OwnerId = ownerId };
        var existingHour = new VenueOperatingHour { DayOfWeek = DayOfWeekEnum.Monday, OpenTime = new TimeSpan(8,0,0), CloseTime = new TimeSpan(22,0,0) };
        
        _userContextMock.Setup(u => u.UserId).Returns(ownerId);
        _venueRepositoryMock.Setup(repo => repo.GetByIdAsync(command.VenueId))
            .ReturnsAsync(venue);
            
        _operatingHourRepositoryMock.Setup(repo => repo.GetByVenueIdAsync(command.VenueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VenueOperatingHour> { existingHour });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        // Since no update happened, verify it wasn't structurally cleared
        _operatingHourRepositoryMock.Verify(repo => repo.Update(It.IsAny<VenueOperatingHour>()), Times.Never);
        _operatingHourRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<VenueOperatingHour>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Covers UTCID12
    [Fact]
    public async Task Handle_BlankTimeStrings_NullifiesTimesAndMarksClosed()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new UpdateOperatingHoursCommand 
        { 
            VenueId = Guid.NewGuid(), 
            OperatingHours = new List<OperatingHourItemDto>
            {
                new OperatingHourItemDto((int)DayOfWeek.Tuesday, "", "")
            }
        };
        
        var venue = new Venue { VenueId = command.VenueId, OwnerId = ownerId };
        var existingHour = new VenueOperatingHour { DayOfWeek = DayOfWeekEnum.Tuesday, OpenTime = new TimeSpan(8,0,0), CloseTime = new TimeSpan(22,0,0), IsClosed = false };
        
        _userContextMock.Setup(u => u.UserId).Returns(ownerId);
        _venueRepositoryMock.Setup(repo => repo.GetByIdAsync(command.VenueId))
            .ReturnsAsync(venue);
            
        _operatingHourRepositoryMock.Setup(repo => repo.GetByVenueIdAsync(command.VenueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VenueOperatingHour> { existingHour });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _operatingHourRepositoryMock.Verify(repo => repo.Update(It.Is<VenueOperatingHour>(h => 
            h.DayOfWeek == DayOfWeekEnum.Tuesday && 
            h.IsClosed == true && 
            h.OpenTime == null && 
            h.CloseTime == null)), Times.Once);
            
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Covers UTCID13
    [Fact]
    public async Task Handle_ValidOperatingTimeInput_UpdatesHoursProperly()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new UpdateOperatingHoursCommand 
        { 
            VenueId = Guid.NewGuid(), 
            OperatingHours = new List<OperatingHourItemDto>
            {
                new OperatingHourItemDto((int)DayOfWeek.Wednesday, "08:00", "22:00")
            }
        };
        
        var venue = new Venue { VenueId = command.VenueId, OwnerId = ownerId };
        var existingHour = new VenueOperatingHour { DayOfWeek = DayOfWeekEnum.Wednesday, OpenTime = new TimeSpan(9,0,0), CloseTime = new TimeSpan(17,0,0), IsClosed = true };
        
        _userContextMock.Setup(u => u.UserId).Returns(ownerId);
        _venueRepositoryMock.Setup(repo => repo.GetByIdAsync(command.VenueId))
            .ReturnsAsync(venue);
            
        _operatingHourRepositoryMock.Setup(repo => repo.GetByVenueIdAsync(command.VenueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VenueOperatingHour> { existingHour });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _operatingHourRepositoryMock.Verify(repo => repo.Update(It.Is<VenueOperatingHour>(h => 
            h.DayOfWeek == DayOfWeekEnum.Wednesday && 
            h.IsClosed == false && 
            h.OpenTime == TimeSpan.Parse("08:00") && 
            h.CloseTime == TimeSpan.Parse("22:00"))), Times.Once);
            
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
