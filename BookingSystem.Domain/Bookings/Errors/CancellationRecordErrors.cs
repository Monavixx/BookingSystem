using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Bookings.Errors;

public static class CancellationRecordErrors
{
    public static readonly DomainError RecordOfThisBookingAlreadyExists =
        new ConflictError("CancellationRecord.RecordOfThisBookingAlreadyExists",
            "A cancellation record for this booking already exists");
}