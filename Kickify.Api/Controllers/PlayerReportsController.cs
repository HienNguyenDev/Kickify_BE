using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.PlayerReports.Commands.ProcessReport;
using Kickify.Application.Features.PlayerReports.Commands.ReportPlayer;
using Kickify.Application.Features.PlayerReports.Queries.GetReports;
using Kickify.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kickify.Api.Controllers;

[ApiController]
[Route("api/player-reports")]
[Authorize]
public class PlayerReportsController : ControllerBase
{
    private readonly ISender _sender;

    public PlayerReportsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Report another player
    /// </summary>
    [HttpPost]
    public async Task<IResult> ReportPlayer(
        [FromBody] ReportPlayerRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseReportType(request.ReportType, out var reportType))
        {
            return Results.BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["reportType"] = ["ReportType is invalid. Use enum name or numeric value."]
            }));
        }

        var command = new ReportPlayerCommand(
            request.ReportedUserId,
            reportType,
            request.Description,
            request.MatchId);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// [Admin] Get all reports with optional filters
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> GetReports(
        [FromQuery] ReportStatus? status = null,
        [FromQuery] Guid? reportedUserId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReportsQuery(status, reportedUserId, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// [Admin] Approve or reject a report
    /// </summary>
    [HttpPatch("{reportId:guid}/process")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> ProcessReport(
        Guid reportId,
        [FromBody] ProcessReportRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ProcessReportCommand(
            reportId,
            request.IsApproved,
            request.AdminNotes,
            request.ActionTaken);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    private static bool TryParseReportType(JsonElement rawReportType, out ReportType reportType)
    {
        reportType = default;

        if (rawReportType.ValueKind == JsonValueKind.Number && rawReportType.TryGetInt32(out var numericValue))
        {
            if (Enum.IsDefined(typeof(ReportType), numericValue))
            {
                reportType = (ReportType)numericValue;
                return true;
            }

            return false;
        }

        if (rawReportType.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var value = rawReportType.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse(value, ignoreCase: true, out reportType);
    }
}
