using Kickify.Api.Extensions;
using Kickify.Application.Features.SystemLogs.Queries.GetSystemLogById;
using Kickify.Application.Features.SystemLogs.Queries.GetSystemLogs;
using Kickify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[ApiController]
[Route("api/system-logs")]
[Authorize(Roles = "Admin")]
public class SystemLogsController : ControllerBase
{
    private readonly ISender _sender;

    public SystemLogsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// [Admin] Paged audit logs with optional filters (max 90-day window).
    /// </summary>
    [HttpGet]
    public async Task<IResult> GetSystemLogs(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? timezone,
        [FromQuery] Guid? userId,
        [FromQuery] SystemLogAction? action,
        [FromQuery] SystemLogResponseStatus? responseStatus,
        [FromQuery] string? entityType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSystemLogsQuery(
            fromDate,
            toDate,
            timezone,
            userId,
            action,
            responseStatus,
            entityType,
            page,
            pageSize);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// [Admin] Single audit log entry with user email when linked user still exists.
    /// </summary>
    [HttpGet("{logId:guid}")]
    public async Task<IResult> GetSystemLogById(Guid logId, CancellationToken cancellationToken)
    {
        var query = new GetSystemLogByIdQuery(logId);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }
}
