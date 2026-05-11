using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.MatchRooms.Commands.CreateMatchRoom;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Moq;
using Xunit;

namespace Kickify.Application.UnitTests.MatchRooms.Commands.CreateMatchRoom;

public class CreateMatchRoomCommandHandlerTests
{
    private readonly Mock<IMatchRoomRepository> _matchRoomRepoMock = new();
    private readonly Mock<IFieldRepository> _fieldRepoMock = new();
    private readonly Mock<IHolidayRepository> _holidayRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IBookingRepository> _bookingRepoMock = new();
    private readonly Mock<IRoomParticipantRepository> _roomParticipantRepoMock = new();
    private readonly Mock<IRoomAutoCloseService> _autoCloseServiceMock = new();
    private readonly Mock<IMatchLifecycleService> _matchLifecycleServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();

    private readonly CreateMatchRoomCommandHandler _sut;

    public CreateMatchRoomCommandHandlerTests()
    {
        _sut = new CreateMatchRoomCommandHandler(
            _matchRoomRepoMock.Object, _fieldRepoMock.Object, _holidayRepoMock.Object, _userRepoMock.Object,
            _bookingRepoMock.Object, _roomParticipantRepoMock.Object, _autoCloseServiceMock.Object,
            _matchLifecycleServiceMock.Object, _unitOfWorkMock.Object, _userContextMock.Object);
    }

    private CreateMatchRoomCommand CreateValidCommand(Guid fieldId, DateTime matchDate, TimeSpan startTime)
    {
        return new CreateMatchRoomCommand(
            FieldId: fieldId,
            RoomName: "Trận giao hữu siêu kinh điển",
            MatchDate: matchDate,
            StartTime: startTime,
            DurationMinutes: 90,
            MatchFormat: "FiveVsFive", // Valid format
            Visibility: "Public",
            Password: null,
            Description: "Đá vui không quạu",
            Rules: "Không xoạc bóng"
        );
    }

    // Covers UTCID17 from CSV
    [Fact]
    public async Task Handle_ArchivedVenue_ReturnsVenueArchivedError_UTCID17()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });

        var field = new Field
        {
            FieldId = fieldId,
            Venue = new Venue { Status = VenueStatus.Archived } // ARCHIVED VENUE
        };

        _fieldRepoMock.Setup(x => x.GetFieldWithVenueAsync(fieldId, It.IsAny<CancellationToken>())).ReturnsAsync(field);

        var command = CreateValidCommand(fieldId, DateTime.UtcNow.Date, TimeSpan.FromHours(18));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(MatchRoomErrors.VenueArchived.Code);
    }

    // Covers UTCID18 from CSV
    [Fact]
    public async Task Handle_OutsideOperatingHours_ReturnsOutsideOperatingHoursError_UTCID18()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var matchDate = DateTime.UtcNow.Date; // Today
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });

        var field = new Field
        {
            FieldId = fieldId,
            Venue = new Venue
            {
                Status = VenueStatus.Approved,
                VenueOperatingHours = new List<VenueOperatingHour>
                {
                    new() {
                        DayOfWeek = (DayOfWeekEnum)matchDate.DayOfWeek,
                        OpenTime = TimeSpan.FromHours(8), // Open at 8 AM
                        CloseTime = TimeSpan.FromHours(22), // Close at 10 PM
                        IsClosed = false
                    }
                }
            }
        };

        _fieldRepoMock.Setup(x => x.GetFieldWithVenueAsync(fieldId, It.IsAny<CancellationToken>())).ReturnsAsync(field);

        // Act: Command requests to start at 7:00 AM (Outside operating hours)
        var command = CreateValidCommand(fieldId, matchDate, TimeSpan.FromHours(7));
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        // Since OutsideOperatingHours is a method that takes parameters, we just check if it contains the root code
        result.Error!.Code.Should().Contain("MatchRoom.OutsideOperatingHours");
    }

    // Covers UTCID19 from CSV
    [Fact]
    public async Task Handle_SlotAlreadyBooked_ReturnsSlotAlreadyBookedError_UTCID19()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var matchDate = DateTime.UtcNow.Date;
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });

        var field = new Field
        {
            FieldId = fieldId,
            Venue = new Venue
            {
                Status = VenueStatus.Approved,
                VenueOperatingHours = new List<VenueOperatingHour>
                {
                    new() { DayOfWeek = (DayOfWeekEnum)matchDate.DayOfWeek, OpenTime = TimeSpan.FromHours(0), CloseTime = TimeSpan.FromHours(23), IsClosed = false }
                }
            }
        };
        _fieldRepoMock.Setup(x => x.GetFieldWithVenueAsync(fieldId, It.IsAny<CancellationToken>())).ReturnsAsync(field);

        // Giả lập slot đã bị người khác đặt (Returns false)
        _bookingRepoMock.Setup(x => x.IsTimeSlotAvailableAsync(fieldId, matchDate, It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = CreateValidCommand(fieldId, matchDate, TimeSpan.FromHours(18));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Contain("MatchRoom.SlotAlreadyBooked");
    }

    // Covers UTCID20 from CSV
    [Fact]
    public async Task Handle_VeryCloseSetup_UsesFallbackDelayOf15Minutes_UTCID20()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();

        // 1. Xác định múi giờ VN để giả lập đúng Input của User
        TimeZoneInfo vnTimeZone;
        try
        {
            vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }

        // 2. Tình huống: Tạo kèo cực sát giờ đá (Ví dụ: đá sau 1 tiếng nữa tính từ hiện tại)
        var utcNow = DateTime.UtcNow;
        var matchStartUtc = utcNow.AddHours(1); // Đá sau 1 tiếng (Hệ UTC)
        var matchStartVn = TimeZoneInfo.ConvertTimeFromUtc(matchStartUtc, vnTimeZone); // Convert sang giờ VN cho Input

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });

        var field = new Field
        {
            FieldId = fieldId,
            HourlyRate = 100000,
            Venue = new Venue
            {
                Status = VenueStatus.Approved,
                VenueOperatingHours = new List<VenueOperatingHour>
                {
                    new() {
                        DayOfWeek = (DayOfWeekEnum)matchStartVn.DayOfWeek,
                        OpenTime = TimeSpan.FromHours(0), 
                        // FIX LỖI: Mở cửa 48 tiếng để không bao giờ dính lỗi OutsideOperatingHours do giờ chạy test
                        CloseTime = TimeSpan.FromHours(48),
                        IsClosed = false
                    }
                }
            }
        };

        _fieldRepoMock.Setup(x => x.GetFieldWithVenueAsync(fieldId, It.IsAny<CancellationToken>())).ReturnsAsync(field);

        _bookingRepoMock.Setup(x => x.IsTimeSlotAvailableAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Slot is available

        // Truyền Input là giờ Việt Nam
        var command = CreateValidCommand(fieldId, matchStartVn.Date, matchStartVn.TimeOfDay);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        // In ra mã lỗi nếu False để lần sau có lỗi mình debug trong 1 nốt nhạc
        result.IsSuccess.Should().BeTrue("because expected success but got error: {0}", result.Error?.Code);

        // Kiểm tra xem Hangfire Job có kích hoạt fallback delay 15 phút không
        // Phải dùng Math.Abs < 1 phút để tránh lỗi timeout vài mili-giây giữa Test và Handler
        _autoCloseServiceMock.Verify(x => x.ScheduleAutoClose(
            It.IsAny<Guid>(),
            It.Is<TimeSpan>(delay => Math.Abs((delay - TimeSpan.FromMinutes(15)).TotalMinutes) < 1)
        ), Times.Once);

        // Kiểm tra db được lưu
        _matchRoomRepoMock.Verify(x => x.AddAsync(It.IsAny<MatchRoom>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Covers UTCID21 from CSV
    [Fact]
    public async Task Handle_NormalSchedule_Over24h_UsesMaxDelayOf24Hours_UTCID21()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();

        // 1. Đồng bộ múi giờ VN
        TimeZoneInfo vnTimeZone;
        try
        {
            vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }

        // 2. Setup trận đấu diễn ra sau 3 ngày
        var utcNow = DateTime.UtcNow;
        var matchStartUtc = utcNow.AddDays(3); // Giờ UTC thực tế
        var matchStartVn = TimeZoneInfo.ConvertTimeFromUtc(matchStartUtc, vnTimeZone); // Convert ra VN để giả lập Input User

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });

        var field = new Field
        {
            FieldId = fieldId,
            HourlyRate = 100000,
            Venue = new Venue
            {
                Status = VenueStatus.Approved,
                VenueOperatingHours = new List<VenueOperatingHour>
                {
                    new() {
                        DayOfWeek = (DayOfWeekEnum)matchStartVn.DayOfWeek,
                        OpenTime = TimeSpan.FromHours(0),
                        CloseTime = TimeSpan.FromHours(48), // Nới giờ để an toàn tuyệt đối
                        IsClosed = false
                    }
                }
            }
        };
        _fieldRepoMock.Setup(x => x.GetFieldWithVenueAsync(fieldId, It.IsAny<CancellationToken>())).ReturnsAsync(field);
        _bookingRepoMock.Setup(x => x.IsTimeSlotAvailableAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Truyền Input hệ VN
        var command = CreateValidCommand(fieldId, matchStartVn.Date, matchStartVn.TimeOfDay);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue("because expected success but got error: {0}", result.Error?.Code);

        // Giới hạn delay max là 24h
        _autoCloseServiceMock.Verify(x => x.ScheduleAutoClose(It.IsAny<Guid>(), TimeSpan.FromHours(24)), Times.Once);

        // Đảm bảo Job nhắc nhở cũng chạy đúng hệ UTC
        _matchLifecycleServiceMock.Verify(x => x.SchedulePreMatchReminders(
            It.IsAny<Guid>(),
            It.Is<DateTime>(dt => Math.Abs((dt - matchStartUtc).TotalMinutes) < 1)
        ), Times.Once);
    }


    // Covers UTCID22 from CSV: Match start time is in the past
    [Fact]
    public async Task Handle_PastTime_ReturnsInvalidTimeError_UTCID22()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });

        TimeZoneInfo vnTimeZone;
        try
        {
            vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }

        var utcNow = DateTime.UtcNow;
        // Giả lập User nhập vào giờ đá là 1 tiếng TRƯỚC (Quá khứ)
        var matchStartUtc = utcNow.AddHours(-1);
        var matchStartVn = TimeZoneInfo.ConvertTimeFromUtc(matchStartUtc, vnTimeZone);

        var field = new Field
        {
            FieldId = fieldId,
            HourlyRate = 100000,
            Venue = new Venue
            {
                Status = VenueStatus.Approved,
                VenueOperatingHours = new List<VenueOperatingHour>
                {
                    new() {
                        DayOfWeek = (DayOfWeekEnum)matchStartVn.DayOfWeek,
                        OpenTime = TimeSpan.FromHours(0),
                        CloseTime = TimeSpan.FromHours(48), // Nới giờ để qua ải OperatingHours
                        IsClosed = false
                    }
                }
            }
        };

        _fieldRepoMock.Setup(x => x.GetFieldWithVenueAsync(fieldId, It.IsAny<CancellationToken>())).ReturnsAsync(field);
        _bookingRepoMock.Setup(x => x.IsTimeSlotAvailableAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = CreateValidCommand(fieldId, matchStartVn.Date, matchStartVn.TimeOfDay);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(MatchRoomErrors.InvalidTime.Code);
    }

    // Covers UTCID23 from CSV: Match start time is too close (less than 30 mins)
    [Fact]
    public async Task Handle_TooCloseToStartTime_ReturnsError_UTCID23()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });

        TimeZoneInfo vnTimeZone;
        try
        {
            vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }

        var utcNow = DateTime.UtcNow;
        // Giả lập User tạo phòng nhưng chỉ cách giờ đá 20 phút (< 30 phút ranh giới)
        var matchStartUtc = utcNow.AddMinutes(20);
        var matchStartVn = TimeZoneInfo.ConvertTimeFromUtc(matchStartUtc, vnTimeZone);

        var field = new Field
        {
            FieldId = fieldId,
            HourlyRate = 100000,
            Venue = new Venue
            {
                Status = VenueStatus.Approved,
                VenueOperatingHours = new List<VenueOperatingHour>
                {
                    new() {
                        DayOfWeek = (DayOfWeekEnum)matchStartVn.DayOfWeek,
                        OpenTime = TimeSpan.FromHours(0),
                        CloseTime = TimeSpan.FromHours(48),
                        IsClosed = false
                    }
                }
            }
        };

        _fieldRepoMock.Setup(x => x.GetFieldWithVenueAsync(fieldId, It.IsAny<CancellationToken>())).ReturnsAsync(field);
        _bookingRepoMock.Setup(x => x.IsTimeSlotAvailableAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = CreateValidCommand(fieldId, matchStartVn.Date, matchStartVn.TimeOfDay);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(MatchRoomErrors.TooCloseToStartTime.Code);
    }


    // Covers multiple time-zone behaviors for Auto-Close Hangfire Job
    [Theory]
    [InlineData(48, 1440)] // Case 1: Tạo trước 48h -> Delay tối đa 24h (24 * 60 = 1440 phút)
    [InlineData(10, 480)]  // Case 2: Tạo trước 10h -> Delay là (10h - 2h) = 8h (8 * 60 = 480 phút)
    [InlineData(1.5, 15)]  // Case 3: Tạo trước 1.5h (Sát giờ) -> timeToMatchStartMinus2h < 0 -> Fallback 15 phút
    [InlineData(2, 15)]    // Điểm biên: Tạo chẵn 2h trước trận -> Fallback 15 phút
    public async Task Handle_CalculatesAutoCloseDelay_CorrectlyForAllTimeZones(double hoursUntilMatch, double expectedDelayMinutes)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();

        // 1. Xác định múi giờ VN 
        TimeZoneInfo vnTimeZone;
        try
        {
            vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }

        // 2. Tính toán giờ giả lập
        var utcNow = DateTime.UtcNow;
        var matchStartUtc = utcNow.AddHours(hoursUntilMatch);
        var matchStartVn = TimeZoneInfo.ConvertTimeFromUtc(matchStartUtc, vnTimeZone);

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });

        var field = new Field
        {
            FieldId = fieldId,
            HourlyRate = 100000,
            Venue = new Venue
            {
                Status = VenueStatus.Approved,
                VenueOperatingHours = new List<VenueOperatingHour>
                {
                    new() {
                        DayOfWeek = (DayOfWeekEnum)matchStartVn.DayOfWeek,
                        OpenTime = TimeSpan.Zero,
                        // FIX LỖI: Mở xuyên màn đêm (48 tiếng) để tránh việc cộng DurationMinutes bị vắt sang ngày mới gây lỗi OutsideOperatingHours
                        CloseTime = TimeSpan.FromHours(48),
                        IsClosed = false
                    }
                }
            }
        };

        _fieldRepoMock.Setup(x => x.GetFieldWithVenueAsync(fieldId, It.IsAny<CancellationToken>())).ReturnsAsync(field);

        _bookingRepoMock.Setup(x => x.IsTimeSlotAvailableAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateMatchRoomCommand(
            FieldId: fieldId,
            RoomName: "Test Auto Close Zones",
            MatchDate: matchStartVn.Date,
            StartTime: matchStartVn.TimeOfDay,
            DurationMinutes: 90,
            MatchFormat: "FiveVsFive",
            Visibility: "Public",
            Password: null,
            Description: "Testing Hangfire Logic",
            Rules: "Fair play"
        );

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        // Chiêu Debug: In thẳng mã lỗi ra test runner nếu bị False để đỡ mất công đoán
        result.IsSuccess.Should().BeTrue("because expected success but got error: {0}", result.Error?.Code);

        var expectedDelay = TimeSpan.FromMinutes(expectedDelayMinutes);

        // Verify Hangfire Job Delay
        _autoCloseServiceMock.Verify(x => x.ScheduleAutoClose(
            It.IsAny<Guid>(),
            It.Is<TimeSpan>(delay => Math.Abs((delay - expectedDelay).TotalMinutes) < 1)
        ), Times.Once, $"Expected delay was {expectedDelayMinutes} minutes but got something else.");

        // Verify PreMatchReminders UTC
        _matchLifecycleServiceMock.Verify(x => x.SchedulePreMatchReminders(
            It.IsAny<Guid>(),
            It.Is<DateTime>(dt => Math.Abs((dt - matchStartUtc).TotalMinutes) < 1)
        ), Times.Once, "PreMatchReminders was not scheduled with the correct UTC time.");
    }
}