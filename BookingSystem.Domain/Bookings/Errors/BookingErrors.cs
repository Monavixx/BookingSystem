using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Bookings.Errors;

public static class BookingErrors
{
    public static readonly DomainError GuestCountOutOfRange =
        new ValidationError("Booking.NumberOfGuests.OutOfRange", "Number of guests must be greater than or equal to 1");
    public static readonly DomainError CapacityExceeded = new UnprocessableEntityError("Booking.NumberOfGuests.CapacityExceeded",
        "The number of guests exceeds the table capacity");
    public static readonly DomainError NotFound = new NotFoundError("Booking.NotFound", "Booking not found");
    public static readonly DomainError AccessDenied = new ForbiddenError("Booking.AccessDenied", "You do not have permission to access this booking");
    public static readonly DomainError TableNotAvailable = new ConflictError(
        "Booking.TableNotAvailable",
        "The selected table is not available for the specified time slot");
    public static readonly DomainError HasBeenChanged = new ConflictError("Booking.HasBeenChanged",
        "The booking has been changed by another process. Please reload the booking and try again.");
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