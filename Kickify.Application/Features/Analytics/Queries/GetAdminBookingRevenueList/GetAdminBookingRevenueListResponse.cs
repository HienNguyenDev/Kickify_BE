using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Analytics.Queries.GetAdminBookingRevenueList;

public record GetAdminBookingRevenueListResponse(
    IReadOnlyList<AdminBookingRevenueListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record AdminBookingRevenueListItemDto(
    Guid BookingId,
    Guid RoomId,
    Guid VenueId,
    string VenueName,
    string FieldName,
    decimal TotalAmount,
    decimal PlatformFee,
    decimal VenueAmount,
    DateTime CompletedAtUtc,
    string BookingStatus,
    string MatchRoomStatus
);
