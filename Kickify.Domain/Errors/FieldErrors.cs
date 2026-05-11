using Kickify.Domain.Common;

namespace Kickify.Domain.Errors
{
    public static class FieldErrors
    {
        public static Error NotFound(Guid? fieldId) => Error.NotFound(
            "Fields.NotFound",
            $"The field with Id = '{fieldId}' was not found");

        public static readonly Error VenueNotFound = Error.NotFound(
            "Fields.VenueNotFound",
            "The venue associated with this field was not found");

        public static readonly Error InvalidHourlyRate = Error.Conflict(
            "Fields.InvalidHourlyRate",
            "Hourly rate must be greater than zero");

        public static readonly Error FieldInactive = Error.Conflict(
            "Fields.FieldInactive",
            "This field is currently inactive and cannot be booked");

        public static readonly Error NotAvailable = Error.Conflict(
            "Fields.NotAvailable",
            "The field is not available for the requested time slot");

        public static readonly Error Unauthorized = Error.Conflict(
            "Fields.Unauthorized",
            "You are not authorized to perform this action on this field");

        public static readonly Error PeakHourOnClosedVenueDay = Error.Conflict(
            "Fields.PeakHourOnClosedVenueDay",
            "Không thể áp dụng giờ cao điểm cho ngày mà khu sân đang đóng cửa.");

        public static readonly Error InvalidPeakHourTimeRange = Error.Problem(
            "Fields.InvalidPeakHourTimeRange",
            "Khung giờ cao điểm không hợp lệ. EndTime phải lớn hơn StartTime.");

        public static readonly Error VenueSuspended = Error.Conflict(
            "Fields.VenueSuspended",
            "The venue this field belongs to is currently suspended");
        public static readonly Error VenueArchived = Error.Problem(
            "Fields.VenueArchived",
            "The venue this field belongs to is currently archived");

        public static readonly Error PeakHourOutsideOperatingHours = Error.Conflict(
            "Fields.PeakHourOutsideOperatingHours",
            "Khung giờ cao điểm phải nằm trong thời gian hoạt động của sân.");
    }
}