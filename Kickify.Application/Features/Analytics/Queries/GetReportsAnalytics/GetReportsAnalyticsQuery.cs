using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Analytics.Queries.GetReportsAnalytics;

public record GetReportsAnalyticsQuery(
    DateTime FromDate,
    DateTime ToDate,
    string? Timezone = null
) : IQuery<GetReportsAnalyticsResponse>;
