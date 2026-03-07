namespace Kickify.Application.Features.PlayerReports.Queries.GetReports;

public record GetReportsResponse(
    List<ReportDto> Reports,
    int Total,
    int Page,
    int PageSize,
    int TotalPages);

public record ReportDto(
    Guid ReportId,
    Guid ReporterId,
    string ReporterName,
    string ReporterEmail,
    Guid ReportedId,
    string ReportedName,
    string ReportedEmail,
    string ReportType,
    string Description,
    string Status,
    string? AdminNotes,
    string? ActionTaken,
    Guid? ResolvedBy,
    DateTime? ResolvedAt,
    DateTime CreatedAt);
