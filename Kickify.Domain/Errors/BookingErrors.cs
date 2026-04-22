using Kickify.Domain.Common;

namespace Kickify.Domain.Errors
{
    public static class BookingErrors
    {
        public static Error NotFound(Guid? bookingId) => Error.NotFound(
            "Bookings.NotFound",
            $"The booking with Id = '{bookingId}' was not found");

        public static Error RoomNotFound(Guid roomId) => Error.NotFound(
            "Bookings.RoomNotFound",
            $"Match room with ID {roomId} not found");

        public static readonly Error ParticipantNotFound = Error.NotFound(
            "Bookings.ParticipantNotFound",
            "User is not a participant of this room");

        public static readonly Error AlreadyPaid = Error.Conflict(
            "Bookings.AlreadyPaid",
            "User has already paid");

        public static readonly Error FieldNotAvailable = Error.Conflict(
            "Bookings.FieldNotAvailable",
            "The selected field is not available for the requested time slot");

        public static readonly Error DoubleBooking = Error.Conflict(
            "Bookings.DoubleBooking",
            "This time slot has just been booked by another room. The field is no longer available");

        public static readonly Error InvalidTimeSlot = Error.Conflict(
            "Bookings.InvalidTimeSlot",
            "Invalid time slot. End time must be after start time");

        public static readonly Error PastBooking = Error.Conflict(
            "Bookings.PastBooking",
            "Cannot create booking for past date/time");

        public static readonly Error InsufficientFunds = Error.Conflict(
            "Bookings.InsufficientFunds",
            "Insufficient funds to complete the booking");

        public static readonly Error RoomNotLocked = Error.Conflict(
            "Bookings.RoomNotLocked",
            "Room must be locked before creating a booking");

        public static readonly Error PaymentNotComplete = Error.Conflict(
            "Bookings.PaymentNotComplete",
            "All participants must complete payment before booking can be created");

        public static readonly Error PaymentProcessFailed = Error.Conflict(
            "Bookings.PaymentProcessFailed",
            "Failed to process payment");

        public static readonly Error NotVenueOwner = Error.Failure(
            "Bookings.NotVenueOwner",
            "You must be a venue owner to access this resource");

        public static Error NotEligibleForRevenueReport(Guid bookingId) => Error.Problem(
            "Bookings.NotEligibleForRevenueReport",
            $"Booking '{bookingId}' does not have completed match revenue (match not finished or booking cancelled).");

        public static Error NoDepositRequired(Guid roomId) => Error.Conflict(
            "Bookings.NoDepositRequired",
            $"Room '{roomId}' does not require a deposit.");

        public static readonly Error PendingCheckInPaymentExists = Error.Conflict(
            "Bookings.PendingCheckInPaymentExists",
            "A pending VNPay check-in payment already exists. Complete or wait for it to expire before creating a new one.");
    }
}
