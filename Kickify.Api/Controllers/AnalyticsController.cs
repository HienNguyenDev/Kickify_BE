using Kickify.Api.Extensions;
using Kickify.Application.Features.Analytics.Queries.GetAdminBookingRevenueList;
using Kickify.Application.Features.Analytics.Queries.GetAdminDashboard;
using Kickify.Application.Features.Analytics.Queries.GetReportsAnalytics;
using Kickify.Application.Features.Analytics.Queries.GetVenueDashboard;
using Kickify.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly ISender _sender;

    public AnalyticsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get venue dashboard analytics (for venue owners)
    /// </summary>
    [Authorize(Roles = "VenueOwner,Admin")]
    [HttpGet("venue-dashboard")]
    public async Task<IResult> GetVenueDashboard(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? timezone,
        [FromQuery] Guid? venueId,
        [FromQuery] int upcomingBookingsPageSize = 10,
        [FromQuery] int recentReviewsPageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var isAdmin = User.IsInRole("Admin");
        var query = new GetVenueDashboardQuery(
            fromDate, toDate, timezone, venueId,
            upcomingBookingsPageSize, recentReviewsPageSize,
            isAdmin);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Get admin dashboard overview
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin-dashboard")]
    public async Task<IResult> GetAdminDashboard(
        [FromQuery] string? timezone,
        [FromQuery] int chartDays = 30,
        [FromQuery] int systemAlertsPageSize = 10,
        [FromQuery] int todayMatchesPageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminDashboardQuery(
            timezone, chartDays, systemAlertsPageSize, todayMatchesPageSize);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Get reports and analytics data (for admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("reports")]
    public async Task<IResult> GetReportsAnalytics(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? timezone,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReportsAnalyticsQuery(fromDate, toDate, timezone);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Platform fee revenue from wallet transactions (BookingCommission + WithdrawalFee + PremiumPurchase).
    /// Optional feeSource filter: BookingCommission | WithdrawalFee | PremiumPurchase (omit for all).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/platform-revenue")]
    public async Task<IResult> GetPlatformFeeRevenue(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? timezone,
        [FromQuery] TransactionType? feeSource = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminBookingRevenueListQuery(
            fromDate, toDate, timezone, feeSource, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }
}
