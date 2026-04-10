using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Features.Fields.Commands.BlockFieldSlot;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Kickify.Application.UnitTests.Fields.Commands.BlockFieldSlot;

public class BlockFieldSlotCommandHandlerTests
{
    private readonly Mock<IFieldRepository> _fieldRepositoryMock = new();
    private readonly Mock<IVenueRepository> _venueRepositoryMock = new();
    private readonly Mock<IMatchRoomRepository> _matchRoomRepositoryMock = new();
    private readonly Mock<IBookingRepository> _bookingRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<ILogger<BlockFieldSlotCommandHandler>> _loggerMock = new();
    private readonly BlockFieldSlotCommandHandler _sut;

    public BlockFieldSlotCommandHandlerTests()
    {
        _sut = new BlockFieldSlotCommandHandler(
            _fieldRepositoryMock.Object,
            _venueRepositoryMock.Object,
            _matchRoomRepositoryMock.Object,
            _bookingRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _userContextMock.Object);
    }

    // Covers UTCID14
    [Fact]
    public async Task Handle_OverlapsWithExistingConfirmedBooking_ReturnsActiveBookingExistsError()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var testDate = DateTime.UtcNow.Date.AddDays(1); // Ensure future date to avoid any other validations
        var command = new BlockFieldSlotCommand(venueId, fieldId, testDate, new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0), "Maintenance", 0);
        
        var venue = new Venue 
        { 
            VenueId = venueId, 
            OwnerId = ownerId,
            VenueOperatingHours = new List<VenueOperatingHour> 
            { 
                new VenueOperatingHour { DayOfWeek = (DayOfWeekEnum)testDate.DayOfWeek, OpenTime = new TimeSpan(8,0,0), CloseTime = new TimeSpan(22,0,0), IsClosed = false }
            }
        };

        var field = new Field { FieldId = fieldId, VenueId = venueId };

        _userContextMock.Setup(u => u.UserId).Returns(ownerId);
        _venueRepositoryMock.Setup(repo => repo.GetVenueWithDetailsAsync(command.VenueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);
            
        _fieldRepositoryMock.Setup(repo => repo.GetFieldWithVenueAsync(command.FieldId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(field);

        // Simulate that the time slot is unavailable (already booked)
        _bookingRepositoryMock.Setup(repo => repo.IsTimeSlotAvailableAsync(command.FieldId, command.Date, command.StartTime, command.EndTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(BlockSlotErrors.SlotAlreadyBooked.Code);
    }

    // Covers UTCID15
    [Fact]
    public async Task Handle_StartsExactlyAsPreviousEnds_LogsPerfectly()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var testDate = DateTime.UtcNow.Date.AddDays(1);
        // Start exactly at 12, assume previous ended at 12. Simulated as "available" by repo logic.
        var command = new BlockFieldSlotCommand(venueId, fieldId, testDate, new TimeSpan(12, 0, 0), new TimeSpan(14, 0, 0), "Off-peak", 0);
        
        var venue = new Venue 
        { 
            VenueId = venueId, 
            OwnerId = ownerId,
            VenueOperatingHours = new List<VenueOperatingHour> 
            { 
                new VenueOperatingHour { DayOfWeek = (DayOfWeekEnum)testDate.DayOfWeek, OpenTime = new TimeSpan(8,0,0), CloseTime = new TimeSpan(22,0,0), IsClosed = false }
            }
        };

        var field = new Field { FieldId = fieldId, VenueId = venueId };

        _userContextMock.Setup(u => u.UserId).Returns(ownerId);
        _venueRepositoryMock.Setup(repo => repo.GetVenueWithDetailsAsync(command.VenueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);
            
        _fieldRepositoryMock.Setup(repo => repo.GetFieldWithVenueAsync(command.FieldId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(field);

        _bookingRepositoryMock.Setup(repo => repo.IsTimeSlotAvailableAsync(command.FieldId, command.Date, command.StartTime, command.EndTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Mock MatchRoom creation / setup defaults if needed inside Handle
        
        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Reason.Should().Be(command.Reason);
        result.Value.StartTime.Should().Be(command.StartTime);

        _bookingRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Booking>(b => 
            b.FieldId == command.FieldId && 
            b.StartTime == command.StartTime && 
            b.EndTime == command.EndTime)), Times.Once);
            
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Covers UTCID16
    [Fact]
    public async Task Handle_CompletelyUntouchedFreeSlot_LogsPerfectly()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var testDate = DateTime.UtcNow.Date.AddDays(1);
        var command = new BlockFieldSlotCommand(venueId, fieldId, testDate, new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0), "Offline tournament", 50);
        
        var venue = new Venue 
        { 
            VenueId = venueId, 
            OwnerId = ownerId,
            VenueOperatingHours = new List<VenueOperatingHour> 
            { 
                new VenueOperatingHour { DayOfWeek = (DayOfWeekEnum)testDate.DayOfWeek, OpenTime = new TimeSpan(8,0,0), CloseTime = new TimeSpan(22,0,0), IsClosed = false }
            }
        };

        var field = new Field { FieldId = fieldId, VenueId = venueId };

        _userContextMock.Setup(u => u.UserId).Returns(ownerId);
        _venueRepositoryMock.Setup(repo => repo.GetVenueWithDetailsAsync(command.VenueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);
            
        _fieldRepositoryMock.Setup(repo => repo.GetFieldWithVenueAsync(command.FieldId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(field);

        _bookingRepositoryMock.Setup(repo => repo.IsTimeSlotAvailableAsync(command.FieldId, command.Date, command.StartTime, command.EndTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Reason.Should().Be(command.Reason);

        _bookingRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Booking>(b => 
            b.FieldId == command.FieldId && 
            b.StartTime == command.StartTime && 
            b.EndTime == command.EndTime && 
            b.Status == BookingStatus.Confirmed)), Times.Once);
            
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
