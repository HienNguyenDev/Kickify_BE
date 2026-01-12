using Kickify.Domain.Common;

namespace Kickify.Domain.Errors
{
    public static class BookingErrors
    {
        public static Error NotFound(Guid? bookingId) => Error.NotFound(
            "Bookings.NotFound",
            $"The booking with Id = '{bookingId}' was not found");

        public static readonly Error FieldNotAvailable = Error.Conflict(
            "Bookings.FieldNotAvailable",
            "The selected field is not available for the requested time slot");

        public static readonly Error DoubleBooking = Error.Conflict(
            "Bookings.DoubleBooking",
            "This time slot has just been booked by another room. The field is no longer available");

        public static readonly Error InvalidTimeSlot = Error.Problem(
            "Bookings.InvalidTimeSlot",
            "Invalid time slot. End time must be after start time");

        public static readonly Error PastBooking = Error.Problem(
            "Bookings.PastBooking",
            "Cannot create booking for past date/time");

        public static readonly Error InsufficientFunds = Error.Problem(
            "Bookings.InsufficientFunds",
            "Insufficient funds to complete the booking");

        public static readonly Error RoomNotLocked = Error.Problem(
            "Bookings.RoomNotLocked",
            "Room must be locked before creating a booking");

        public static readonly Error PaymentNotComplete = Error.Problem(
            "Bookings.PaymentNotComplete",
            "All participants must complete payment before booking can be created");
    }
}
