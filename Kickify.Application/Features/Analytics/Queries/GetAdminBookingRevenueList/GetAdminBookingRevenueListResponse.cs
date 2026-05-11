namespace Kickify.Application.Features.Analytics.Queries.GetAdminBookingRevenueList;

public record GetAdminBookingRevenueListResponse(
    decimal TotalPlatformFee,
    decimal BookingCommissionTotal,
    decimal WithdrawalFeeTotal,
    decimal PremiumPurchaseTotal,
    IReadOnlyList<PlatformFeeTransactionDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record PlatformFeeTransactionDto(
    Guid TransactionId,
    string FeeSource,
    decimal Amount,
    Guid? ReferenceId,
    string? Description,
    DateTime CreatedAtUtc
);
