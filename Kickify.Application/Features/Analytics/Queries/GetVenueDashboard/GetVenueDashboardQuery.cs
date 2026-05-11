using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Analytics.Queries.GetVenueDashboard;

public record GetVenueDashboardQuery(
    DateTime FromDate,
    DateTime ToDate,
    string? Timezone = null,
    Guid? VenueId = null,
    int UpcomingBookingsPageSize = 10,
    int RecentReviewsPageSize = 10,
    bool IsAdmin = false
) : IQuery<GetVenueDashboardResponse>;
