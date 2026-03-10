using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.ContentReports.Commands.ProcessContentReport;
using Kickify.Application.Features.ContentReports.Commands.ReportContent;
using Kickify.Application.Features.ContentReports.Queries.GetContentReports;
using Kickify.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[ApiController]
[Route("api/content-reports")]
[Authorize]
public class ContentReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ContentReportsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Report a post or comment
    /// </summary>
    [HttpPost("{contentType}/{contentId:guid}")]
    public async Task<IResult> ReportContent(
        ContentType contentType,
        Guid contentId,
        [FromBody] ReportContentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReportContentCommand
        {
            ContentType = contentType,
            ContentId = contentId,
            Reason = request.Reason,
            Description = request.Description
        };
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// [Admin] Get all content reports with optional filters
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> GetContentReports(
        [FromQuery] ReportStatus? status = null,
        [FromQuery] ContentType? contentType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetContentReportsQuery
        {
            Status = status,
            ContentType = contentType,
            Page = page,
            PageSize = pageSize
        };
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// [Admin] Resolve or dismiss a content report
    /// </summary>
    [HttpPatch("{reportId:guid}/process")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> ProcessContentReport(
        Guid reportId,
        [FromBody] ProcessContentReportRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ProcessContentReportCommand
        {
            ReportId = reportId,
            Action = request.Action,
            AdminNotes = request.AdminNotes
        };
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
