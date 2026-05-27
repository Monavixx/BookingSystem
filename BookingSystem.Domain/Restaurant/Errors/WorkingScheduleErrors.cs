using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Restaurant.Errors;

public static class WorkingScheduleErrors
{
    public static readonly DomainError DaysOutOfRange =
        new ValidationError("Restaurant.WorkingSchedule.DaysLengthOutOfRange", "Working schedule must contain exactly 7 day of week schedules");
}