using FluentAssertions;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.PlayerReports.Commands.ProcessReport;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.PlayerReports.Commands.ProcessReport;

public class ProcessReportCommandHandlerTests
{
    private readonly Mock<IPlayerReportRepository> _reportRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly ProcessReportCommandHandler _handler;

    public ProcessReportCommandHandlerTests()
    {
        _reportRepositoryMock = new Mock<IPlayerReportRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userContextMock = new Mock<IUserContext>();

        _handler = new ProcessReportCommandHandler(
            _reportRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object);
    }

    // Covers UTCID50
    [Fact]
    public async Task Handle_WhenReportDoesNotExist_ShouldReturnFailure_UTCID50()
    {
        // Arrange
        var command = new ProcessReportCommand(Guid.NewGuid(), true);
        _reportRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(command.ReportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerReport)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(PlayerReportErrors.NotFound(command.ReportId));
    }

    // Covers UTCID51
    [Fact]
    public async Task Handle_WhenReportAlreadyProcessed_ShouldReturnFailure_UTCID51()
    {
        // Arrange
        var command = new ProcessReportCommand(Guid.NewGuid(), true);
        var report = new PlayerReport { ReportId = command.ReportId, Status = ReportStatus.Resolved };
        _reportRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(command.ReportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(PlayerReportErrors.AlreadyProcessed);
    }

    // Covers UTCID52
    [Fact]
    public async Task Handle_WhenDismissed_ShouldUpdateStatusToDismissed_UTCID52()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var command = new ProcessReportCommand(Guid.NewGuid(), false, "No issue found");
        var report = new PlayerReport { ReportId = command.ReportId, Status = ReportStatus.Pending };

        _reportRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(command.ReportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);
        _userContextMock.SetupGet(u => u.UserId).Returns(adminId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        report.Status.Should().Be(ReportStatus.Dismissed);
        report.AdminNotes.Should().Be("No issue found");
        report.ResolvedBy.Should().Be(adminId);
        
        _reportRepositoryMock.Verify(r => r.Update(report), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Covers UTCID53
    [Fact]
    public async Task Handle_WhenApproved_ShouldUpdateStatusToResolved_UTCID53()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var command = new ProcessReportCommand(Guid.NewGuid(), true, "Severe violation", "User suspended");
        var report = new PlayerReport { ReportId = command.ReportId, Status = ReportStatus.Pending };

        _reportRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(command.ReportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);
        _userContextMock.SetupGet(u => u.UserId).Returns(adminId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        report.Status.Should().Be(ReportStatus.Resolved);
        report.AdminNotes.Should().Be("Severe violation");
        report.ActionTaken.Should().Be("User suspended");
        report.ResolvedBy.Should().Be(adminId);
        
        _reportRepositoryMock.Verify(r => r.Update(report), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
