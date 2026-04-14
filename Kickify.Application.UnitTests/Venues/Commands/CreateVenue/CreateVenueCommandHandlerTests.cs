using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Venues.Commands.CreateVenue;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Kickify.Application.UnitTests.Venues.Commands.CreateVenue
{
    public class CreateVenueCommandHandlerTests
    {
        private readonly Mock<IVenueRepository> _venueRepositoryMock;
        private readonly Mock<IHolidayRepository> _holidayRepositoryMock;
        private readonly Mock<IWalletRepository> _walletRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserContext> _userContextMock;
        private readonly Mock<ILogger<CreateVenueCommandHandler>> _loggerMock;
        private readonly CreateVenueCommandHandler _handler;

        public CreateVenueCommandHandlerTests()
        {
            _venueRepositoryMock = new Mock<IVenueRepository>();
            _holidayRepositoryMock = new Mock<IHolidayRepository>();
            _walletRepositoryMock = new Mock<IWalletRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userContextMock = new Mock<IUserContext>();
            _loggerMock = new Mock<ILogger<CreateVenueCommandHandler>>();

            _handler = new CreateVenueCommandHandler(
                _venueRepositoryMock.Object,
                _holidayRepositoryMock.Object,
                _walletRepositoryMock.Object,
                _userRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _userContextMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure_UTCID01()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            _userContextMock.Setup(c => c.UserId).Returns(ownerId);
            
            var command = new CreateVenueCommand(
                "Venue 1", "Address 1", 10.0m, 20.0m, "123", "a@a.com", "Desc", "WiFi", 
                new List<Guid>(), new List<CreateVenueFieldDto>(), new List<CreateVenueOperatingHoursDto>());

            _userRepositoryMock.Setup(r => r.GetByIdAsync(ownerId)).ReturnsAsync((User)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(UserErrors.NotFound(ownerId));
            
            _venueRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Venue>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidHolidayId_ReturnsFailure_UTCID02()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            _userContextMock.Setup(c => c.UserId).Returns(ownerId);
            
            var user = new User { UserId = ownerId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(ownerId)).ReturnsAsync(user);

            var invalidHolidayId = Guid.NewGuid();
            var validHolidayId = Guid.NewGuid();
            var command = new CreateVenueCommand(
                "Venue 1", "Address 1", 10.0m, 20.0m, "123", "a@a.com", "Desc", "WiFi", 
                new List<Guid> { validHolidayId, invalidHolidayId }, 
                new List<CreateVenueFieldDto>(), new List<CreateVenueOperatingHoursDto>());

            var foundHolidays = new List<Holiday> { new Holiday { Id = validHolidayId } };
            _holidayRepositoryMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(foundHolidays);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(HolidayErrors.InvalidIds(new List<Guid> { invalidHolidayId }));
            
            _venueRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Venue>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidFieldTypeString_ReturnsFailure_UTCID03()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            _userContextMock.Setup(c => c.UserId).Returns(ownerId);
            
            var user = new User { UserId = ownerId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(ownerId)).ReturnsAsync(user);

            var command = new CreateVenueCommand(
                "Venue 1", "Address 1", 10.0m, 20.0m, "123", "a@a.com", "Desc", "WiFi", 
                new List<Guid>(), 
                new List<CreateVenueFieldDto> { 
                    new CreateVenueFieldDto("Field 1", "InvalidType", "Turf", 100, 20, null, null, 10, 10, null, false, false, false) 
                }, 
                new List<CreateVenueOperatingHoursDto>());

            _holidayRepositoryMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Holiday>());
                
            _venueRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Venue>())).Returns(Task.CompletedTask);
            _walletRepositoryMock.Setup(r => r.GetByUserIdAsync(ownerId, It.IsAny<CancellationToken>())).ReturnsAsync((Wallet)null);
            _walletRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Wallet>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(VenueErrors.InvalidFieldType("InvalidType"));
            
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ValidData_ReturnsSuccess_UTCID04()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            _userContextMock.Setup(c => c.UserId).Returns(ownerId);
            
            var user = new User { UserId = ownerId };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(ownerId)).ReturnsAsync(user);

            var command = new CreateVenueCommand(
                "Venue 1", "Address 1", 10.0m, 20.0m, "123", "a@a.com", "Desc", "WiFi", 
                new List<Guid>(), 
                new List<CreateVenueFieldDto> { 
                    new CreateVenueFieldDto("Field 1", "FiveVsFive", "Turf", 100, 20, null, null, 10, 10, null, false, false, false) 
                }, 
                new List<CreateVenueOperatingHoursDto> {
                    new CreateVenueOperatingHoursDto((int)DayOfWeekEnum.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(22), false)
                });

            _holidayRepositoryMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Holiday>());
                
            _venueRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Venue>())).Returns(Task.CompletedTask);
            
            var walletId = Guid.NewGuid();
            var wallet = new Wallet { WalletId = walletId, UserId = ownerId };
            _walletRepositoryMock.Setup(r => r.GetByUserIdAsync(ownerId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be("Venue 1");
            result.Value.WalletId.Should().Be(walletId);
            result.Value.Fields.Should().HaveCount(1);
            result.Value.Fields.First().FieldType.Should().Be("FiveVsFive");
            
            _venueRepositoryMock.Verify(r => r.AddAsync(It.Is<Venue>(v => 
                v.VenueName == "Venue 1" && 
                v.OwnerId == ownerId &&
                v.Fields.Count == 1 &&
                v.VenueOperatingHours.Count == 1)), Times.Once);
                
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}