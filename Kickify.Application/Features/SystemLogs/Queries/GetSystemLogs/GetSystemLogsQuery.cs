using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Entities;

namespace Kickify.Application.Features.SystemLogs.Queries.GetSystemLogs;

public record GetSystemLogsQuery(
    DateTime FromDate,
    DateTime ToDate,
    string? Timezone = null,
    Guid? UserId = null,
    SystemLogAction? Action = null,
    SystemLogResponseStatus? ResponseStatus = null,
    string? EntityType = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<GetSystemLogsResponse>;
