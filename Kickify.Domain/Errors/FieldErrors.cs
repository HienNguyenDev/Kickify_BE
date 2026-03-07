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

        public static readonly Error InvalidHourlyRate = Error.Problem(
            "Fields.InvalidHourlyRate",
            "Hourly rate must be greater than zero");

        public static readonly Error FieldInactive = Error.Problem(
            "Fields.FieldInactive",
            "This field is currently inactive and cannot be booked");

        public static readonly Error NotAvailable = Error.Conflict(
            "Fields.NotAvailable",
            "The field is not available for the requested time slot");

        public static readonly Error Unauthorized = Error.Problem(
            "Fields.Unauthorized",
            "You are not authorized to perform this action on this field");

        public static readonly Error VenueSuspended = Error.Problem(
            "Fields.VenueSuspended",
            "The venue this field belongs to is currently suspended");
    }
}
