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

    public static int[] FinalStatuses { get; } = _finalStatuses.Cast<int>().ToArray();

    private static readonly FrozenSet<BookingStatus> FinalStatusesSet = _finalStatuses.ToFrozenSet();
    
    public static bool IsFinal(this BookingStatus bookingStatus) =>
        FinalStatusesSet.Contains(bookingStatus);
}