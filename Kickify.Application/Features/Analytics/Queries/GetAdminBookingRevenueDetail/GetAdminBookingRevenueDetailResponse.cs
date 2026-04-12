using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Analytics.Queries.GetAdminBookingRevenueDetail;

public record GetAdminBookingRevenueDetailResponse(
    Guid BookingId,
    Guid RoomId,
    Guid VenueId,
    string VenueName,
    string VenueAddress,
    Guid FieldId,
    string FieldName,
    FieldType FieldType,
    decimal TotalAmount,
    decimal PlatformFee,
    decimal VenueAmount,
    BookingStatus BookingStatus,
    RoomStatus MatchRoomStatus,
    DateTime BookingDateUtc,
    TimeSpan StartTime,
    TimeSpan EndTime,
    DateTime MatchCompletedAtUtc,
    string? PaymentMethod,
    string? TransactionReference,
    Guid HostUserId,
    string HostName
);
