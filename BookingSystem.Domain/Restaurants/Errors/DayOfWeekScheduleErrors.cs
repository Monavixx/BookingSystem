using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Restaurants.Errors;

public static class DayOfWeekScheduleErrors
{
    public static readonly DomainError AmbiguousSchedule = new ValidationError("Restaurant.DayOfWeekSchedule.AmbiguousSchedule",
        "Opening and closing times must be null if the restaurant is closed, and must be not null if the restaurant is open");
}