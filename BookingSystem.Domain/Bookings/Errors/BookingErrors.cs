using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Bookings.Errors;

public static class BookingErrors
{
    public static readonly DomainError GuestCountOutOfRange =
        new ValidationError("Booking.NumberOfGuests.OutOfRange", "Number of guests must be greater than or equal to 1");
    public static readonly DomainError ScheduledAtInThePast =
        new ValidationError("Booking.ScheduledAt.InThePast", "Scheduled time must be in the future");
    public static readonly DomainError CapacityExceeded = new UnprocessableEntityError("Booking.NumberOfGuests.CapacityExceeded",
        "The number of guests exceeds the table capacity");
    
    public static class TimeSlot
    {
        public static readonly DomainError InvalidTimeRange =
            new ValidationError("Booking.TimeSlot.InvalidTimeRange", "Start time must precede end time");
    }

    public static class Status
    {
        public static readonly DomainError InvalidStatusTransition = new ConflictError(
            "Booking.InvalidStatusTransition",
            "Booking cannot be cancelled since it is already completed");
    }
}