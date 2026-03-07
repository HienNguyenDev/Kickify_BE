using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.PlayerReports.Commands.ProcessReport;

public record ProcessReportCommand(
    Guid ReportId,
    bool IsApproved,
    string? AdminNotes = null,
    string? ActionTaken = null) : ICommand<ProcessReportResponse>;
