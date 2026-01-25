using Kickify.Domain.Common;

namespace Kickify.Domain.Errors
{
    public static class BlockSlotErrors
    {
        public static readonly Error InvalidTimeRange = Error.Problem(
            "BlockSlot.InvalidTimeRange",
            "End time must be greater than start time");

        public static readonly Error SlotAlreadyBooked = Error.Conflict(
            "BlockSlot.SlotAlreadyBooked",
            "The requested time slot is already booked or blocked");

        public static readonly Error FieldNotFound = Error.NotFound(
            "BlockSlot.FieldNotFound",
            "The specified field was not found");

        public static readonly Error VenueNotFound = Error.NotFound(
            "BlockSlot.VenueNotFound",
            "The specified venue was not found");

        public static readonly Error Unauthorized = Error.Problem(
            "BlockSlot.Unauthorized",
            "You are not authorized to block slots on this field. Only the venue owner can perform this action");

        public static readonly Error FieldNotBelongToVenue = Error.Problem(
            "BlockSlot.FieldNotBelongToVenue",
            "The specified field does not belong to the specified venue");

        public static readonly Error OutsideOperatingHours = Error.Problem(
            "BlockSlot.OutsideOperatingHours",
            "The requested time slot is outside venue operating hours");

        public static readonly Error VenueClosedOnDay = Error.Problem(
            "BlockSlot.VenueClosedOnDay",
            "The venue is closed on the specified day");

        public static Error DatabaseConflict(string message) => Error.Conflict(
            "BlockSlot.DatabaseConflict",
            message);
    }
}
