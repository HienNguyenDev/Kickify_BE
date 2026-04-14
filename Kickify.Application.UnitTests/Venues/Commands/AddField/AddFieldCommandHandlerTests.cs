using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Venues.Commands.AddField;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Moq;
using Xunit;

namespace Kickify.Application.UnitTests.Venues.Commands.AddField
{
    public class AddFieldCommandHandlerTests
    {
        private readonly Mock<IVenueRepository> _venueRepositoryMock;
        private readonly Mock<IFieldRepository> _fieldRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly AddFieldCommandHandler _handler;

        public AddFieldCommandHandlerTests()
        {
            _venueRepositoryMock = new Mock<IVenueRepository>();
            _fieldRepositoryMock = new Mock<IFieldRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new AddFieldCommandHandler(
                _venueRepositoryMock.Object,
                _fieldRepositoryMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_TargetVenueNotFound_ReturnsFailure_UTCID05()
        {
            // Arrange
            var command = new AddFieldCommand(
                Guid.NewGuid(), "Field 1", "FiveVsFive", "ArtificialGrass", 100, 20, null, null, 10, 10, null, false, false, false);

            _venueRepositoryMock.Setup(r => r.GetVenueWithDetailsAsync(command.VenueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Venue)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(VenueErrors.NotFound(command.VenueId));
            
            _fieldRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Field>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidFieldTypeString_ReturnsFailure_UTCID06()
        {
            // Arrange
            var command = new AddFieldCommand(
                Guid.NewGuid(), "Field 1", "InvalidType", "ArtificialGrass", 100, 20, null, null, 10, 10, null, false, false, false);
            
            var venue = new Venue { VenueId = command.VenueId };
            
            _venueRepositoryMock.Setup(r => r.GetVenueWithDetailsAsync(command.VenueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(venue);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(VenueErrors.InvalidFieldType("InvalidType"));
            
            _fieldRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Field>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ValidFieldTypeString_ReturnsSuccess_UTCID07()
        {
            // Arrange
            var command = new AddFieldCommand(
                Guid.NewGuid(), "Field 1", "SevenVsSeven", "NaturalGrass", 150, 30, TimeSpan.FromHours(18), TimeSpan.FromHours(20), 20, 20, null, false, false, false);
            
            var venue = new Venue 
            { 
                VenueId = command.VenueId,
                VenueOperatingHours = new List<VenueOperatingHour>
                {
                    new VenueOperatingHour { DayOfWeek = DayOfWeekEnum.Monday, IsClosed = false },
                    new VenueOperatingHour { DayOfWeek = DayOfWeekEnum.Tuesday, IsClosed = false },
                    new VenueOperatingHour { DayOfWeek = DayOfWeekEnum.Sunday, IsClosed = true }
                }
            };
            
            _venueRepositoryMock.Setup(r => r.GetVenueWithDetailsAsync(command.VenueId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(venue);
                
            _fieldRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Field>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.VenueId.Should().Be(command.VenueId);
            result.Value.FieldType.Should().Be("SevenVsSeven");
            
            var expectedPeakDays = new List<DayOfWeekEnum> { DayOfWeekEnum.Monday, DayOfWeekEnum.Tuesday };
            result.Value.PeakDaysOfWeek.Should().BeEquivalentTo(expectedPeakDays);
            
            _fieldRepositoryMock.Verify(r => r.AddAsync(It.Is<Field>(f => 
                f.VenueId == command.VenueId && 
                f.FieldName == "Field 1" &&
                f.FieldType == FieldType.SevenVsSeven &&
                f.PeakDaysOfWeek.SequenceEqual(expectedPeakDays)
            )), Times.Once);
            
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}