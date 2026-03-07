namespace Kickify.Application.Features.PlayerReports.Commands.ProcessReport;

public record ProcessReportResponse(
    Guid ReportId,
    string Status,
    string? AdminNotes,
    string? ActionTaken,
    DateTime ResolvedAt);
