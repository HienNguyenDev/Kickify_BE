using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Analytics.Queries.GetAdminBookingRevenueList;

/// <summary>
/// Query platform fee revenue from wallet transactions.
/// feeSource = null means all three types; otherwise filter to one specific type.
/// </summary>
public record GetAdminBookingRevenueListQuery(
    DateTime FromDate,
    DateTime ToDate,
    string? Timezone = null,
    TransactionType? FeeSource = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<GetAdminBookingRevenueListResponse>;
