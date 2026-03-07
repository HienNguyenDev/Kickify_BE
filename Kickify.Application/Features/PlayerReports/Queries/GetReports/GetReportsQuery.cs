using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.PlayerReports.Queries.GetReports;

public record GetReportsQuery(
    ReportStatus? Status = null,
    Guid? ReportedUserId = null,
    int Page = 1,
    int PageSize = 10) : IQuery<GetReportsResponse>;
