namespace BookingSystem.Domain.Bookings.ValueObjects;

public enum BookingStatus
{
    /// <summary>
    /// Just created and waiting for the guest confirmation.
    /// </summary>
    Pending,
    /// <summary>
    /// The guest confirmed the correctness of the data they provided, the rules of the service and the duration they are allowed to take the table.
    /// </summary>
    ConfirmedByGuest,
    /// <summary>
    /// The restaurant confirmed the booking.
    /// </summary>
    Confirmed,
    /// <summary>
    /// The guest came in the restaurant.
    /// </summary>
    Seated,
    /// <summary>
    /// Either the guest or the restaurant canceled the booking.
    /// </summary>
    Canceled,
    /// <summary>
    /// Either the guests finished and left the restaurant or their time ran out.
    /// </summary>
    Completed,
    NoShow
}