namespace BookingSystem.Domain.Bookings.ValueObjects;

public enum CancellationReason
{
    /// <summary>Guest canceled their own booking.</summary>
    GuestRequest,

    /// <summary>Manager or admin canceled without any guest involvement.</summary>
    ManagerOrAdminRequest,

    /// <summary>Manager or admin canceled on guest's behalf (e.g. guest called the restaurant).</summary>
    ManagerOrAdminBeenAskedByGuest,

    /// <summary>Guest never confirmed the booking before the pending timeout.</summary>
    PendingTimeout,

    /// <summary>Manager never confirmed a guest-confirmed booking in time.</summary>
    ManagerHasNotConfirmed,

    /// <summary>Guest didn't show up within the no-show window.</summary>
    NoShow
}