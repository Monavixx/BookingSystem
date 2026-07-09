using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Common;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Domain.Bookings;

public class CancellationRecord : Entity<Guid>
{
    private CancellationRecord()
    {
    }

    public UserId? WhoCancelledId { get; private set; }
    public BookingId? BookingId { get; private set; }
    public DateTimeOffset CanceledAt { get; private set; }
    public CancellationReason Reason { get; private set; }

    public static CancellationRecord Create(TimeProvider timeProvider, UserId? whoCancelled, BookingId bookingId,
        CancellationReason reason)
    {
        return new CancellationRecord
        {
            Id = Guid.CreateVersion7(),
            WhoCancelledId = whoCancelled,
            BookingId = bookingId,
            CanceledAt = timeProvider.GetUtcNow(),
            Reason = reason
        };
    }
}