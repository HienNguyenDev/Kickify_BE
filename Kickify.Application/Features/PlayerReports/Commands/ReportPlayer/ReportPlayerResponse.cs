namespace Kickify.Application.Features.PlayerReports.Commands.ReportPlayer;

public record ReportPlayerResponse(
    Guid ReportId,
    Guid ReportedUserId,
    string ReportType,
    string Description,
    string Status,
    DateTime CreatedAt);
