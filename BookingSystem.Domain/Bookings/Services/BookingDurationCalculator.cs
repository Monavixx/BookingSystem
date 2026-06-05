using BookingSystem.Domain.Bookings.Errors;
using FluentResults;

namespace BookingSystem.Domain.Bookings.Services;

public class BookingDurationCalculator
{
    private const int BookingTimeForSmallGroupsInMinutes = 90;
    private const int BookingTimeForLargeGroupsInMinutes = 120;
    private const int MaxGroupSizeForSmallGroups = 4;

    public Result<TimeSpan> CalculateDuration(int guestCount)
        => guestCount <= 0
            ? Result.Fail<TimeSpan>(BookingErrors.GuestCountOutOfRange)
            : TimeSpan.FromMinutes(guestCount > MaxGroupSizeForSmallGroups
                ? BookingTimeForLargeGroupsInMinutes
                : BookingTimeForSmallGroupsInMinutes);
}