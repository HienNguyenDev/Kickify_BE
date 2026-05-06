using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.Analytics.Queries.GetAdminBookingRevenueList;

/// <summary>
/// Returns platform fee revenue aggregated from wallet transactions of type
/// BookingCommission, WithdrawalFee, and PremiumPurchase.
/// </summary>
public class GetAdminBookingRevenueListQueryHandler
    : IQueryHandler<GetAdminBookingRevenueListQuery, GetAdminBookingRevenueListResponse>
{
    private readonly IApplicationDbContext _db;
    private const string DefaultTimezone = "Asia/Ho_Chi_Minh";

    private static readonly TransactionType[] PlatformFeeTypes =
    [
        TransactionType.BookingCommission,
        TransactionType.WithdrawalFee,
        TransactionType.PremiumPurchase
    ];

    public GetAdminBookingRevenueListQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<GetAdminBookingRevenueListResponse>> Handle(
        GetAdminBookingRevenueListQuery request, CancellationToken cancellationToken)
    {
        var tz = ResolveTimezone(request.Timezone);
        var fromUtc = ToUtcBoundary(request.FromDate.Date, tz);
        var toUtcExclusive = ToUtcBoundary(request.ToDate.Date.AddDays(1), tz);

        // Base: all platform-fee transactions in the date range
        var baseQuery = _db.WalletTransactions
            .AsNoTracking()
            .Where(t => PlatformFeeTypes.Contains(t.TransactionType)
                     && t.CreatedAt >= fromUtc
                     && t.CreatedAt < toUtcExclusive);

        // Optional source filter
        if (request.FeeSource.HasValue)
            baseQuery = baseQuery.Where(t => t.TransactionType == request.FeeSource.Value);

        // Aggregates across the full (unfiltered-by-source) date range for breakdown totals
        var allInRange = _db.WalletTransactions
            .AsNoTracking()
            .Where(t => PlatformFeeTypes.Contains(t.TransactionType)
                     && t.CreatedAt >= fromUtc
                     && t.CreatedAt < toUtcExclusive);

        var bookingCommissionTotal = await allInRange
            .Where(t => t.TransactionType == TransactionType.BookingCommission)
            .SumAsync(t => -t.Amount, cancellationToken);  // stored as negative

        var withdrawalFeeTotal = await allInRange
            .Where(t => t.TransactionType == TransactionType.WithdrawalFee)
            .SumAsync(t => -t.Amount, cancellationToken);

        var premiumTotal = await allInRange
            .Where(t => t.TransactionType == TransactionType.PremiumPurchase)
            .SumAsync(t => -t.Amount, cancellationToken);

        var totalPlatformFee = bookingCommissionTotal + withdrawalFeeTotal + premiumTotal;

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new PlatformFeeTransactionDto(
                t.TransactionId,
                t.TransactionType.ToString(),
                -t.Amount,          // stored negative, expose as positive fee value
                t.ReferenceId,
                t.Description,
                t.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetAdminBookingRevenueListResponse(
            totalPlatformFee,
            bookingCommissionTotal,
            withdrawalFeeTotal,
            premiumTotal,
            items,
            totalCount,
            request.Page,
            request.PageSize));
    }

    private static DateTime ToUtcBoundary(DateTime localDate, TimeZoneInfo tz) =>
        TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified), tz);

    private static TimeZoneInfo ResolveTimezone(string? timezone)
    {
        if (string.IsNullOrWhiteSpace(timezone))
            return TimeZoneInfo.FindSystemTimeZoneById(DefaultTimezone);
        try { return TimeZoneInfo.FindSystemTimeZoneById(timezone); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById(DefaultTimezone); }
    }
}
