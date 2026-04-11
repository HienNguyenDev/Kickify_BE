using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Analytics.Queries.GetAdminBookingRevenueDetail;

public record GetAdminBookingRevenueDetailQuery(Guid BookingId) : IQuery<GetAdminBookingRevenueDetailResponse>;
