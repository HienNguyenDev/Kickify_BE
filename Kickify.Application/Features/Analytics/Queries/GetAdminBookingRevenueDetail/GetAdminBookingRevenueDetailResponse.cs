namespace Kickify.Application.Features.Analytics.Queries.GetAdminBookingRevenueDetail;

/// <param name="BookingDateUtc">UTC instant when the booked slot starts (VN local date + start time → UTC).</param>
public record GetAdminBookingRevenueDetailResponse(
    Guid BookingId,
    Guid RoomId,
    Guid VenueId,
    string VenueName,
    string VenueAddress,
    Guid FieldId,
    string FieldName,
    string FieldType,
    decimal TotalAmount,
    decimal PlatformFee,
    decimal VenueAmount,
    string BookingStatus,
    string MatchRoomStatus,
    DateTime BookingDateUtc,
    TimeSpan StartTime,
    TimeSpan EndTime,
    DateTime MatchCompletedAtUtc,
    string? PaymentMethod,
    string? TransactionReference,
    Guid HostUserId,
    string HostName
);
