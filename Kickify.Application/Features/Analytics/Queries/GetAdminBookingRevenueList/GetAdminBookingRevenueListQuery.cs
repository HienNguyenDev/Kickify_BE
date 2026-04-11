using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Analytics.Queries.GetAdminBookingRevenueList;

public record GetAdminBookingRevenueListQuery(
    DateTime FromDate,
    DateTime ToDate,
    string? Timezone = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<GetAdminBookingRevenueListResponse>;
