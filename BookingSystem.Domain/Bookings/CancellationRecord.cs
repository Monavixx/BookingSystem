using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Common;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Domain.Bookings;

public class CancellationRecord : Entity<Guid>
{
    private CancellationRecord(){}
    
    public UserId? WhoCancelledId { get; set; }
    public BookingId? BookingId { get; set; }
    public DateTimeOffset CanceledAt { get; set; }
    
    public static CancellationRecord Create(TimeProvider timeProvider, UserId? whoCancelled, BookingId bookingId)
    {
        return new CancellationRecord
        {
            Id = Guid.CreateVersion7(),
            WhoCancelledId = whoCancelled,
            BookingId = bookingId,
            CanceledAt = timeProvider.GetUtcNow()
        };
    }
}