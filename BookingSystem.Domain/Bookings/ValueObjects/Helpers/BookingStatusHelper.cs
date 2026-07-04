using System.Collections.Frozen;

namespace BookingSystem.Domain.Bookings.ValueObjects.Helpers;

public static class BookingStatusHelper
{
    private static readonly BookingStatus[] _finalStatuses =
    [
        BookingStatus.Canceled,
        BookingStatus.Completed,
        BookingStatus.NoShow
    ];

    public static int[] FinalIntStatuses { get; } = _finalStatuses.Cast<int>().ToArray();

    public static readonly FrozenSet<BookingStatus> FinalStatuses = _finalStatuses.ToFrozenSet();
    
    public static bool IsFinal(this BookingStatus bookingStatus) =>
        FinalStatuses.Contains(bookingStatus);
}