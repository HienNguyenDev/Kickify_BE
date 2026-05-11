using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Analytics.Queries.GetAdminDashboard;

public record GetAdminDashboardQuery(
    string? Timezone = null,
    int ChartDays = 30,
    int SystemAlertsPageSize = 10,
    int TodayMatchesPageSize = 10
) : IQuery<GetAdminDashboardResponse>;
