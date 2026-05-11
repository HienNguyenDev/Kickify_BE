using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.PlayerReports.Commands.ReportPlayer;

public record ReportPlayerCommand(
    Guid ReportedUserId,
    ReportType ReportType,
    string Description,
    Guid? MatchId = null) : ICommand<ReportPlayerResponse>;
