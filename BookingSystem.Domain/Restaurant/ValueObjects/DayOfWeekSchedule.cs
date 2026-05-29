using BookingSystem.Domain.Restaurant.Errors;
using FluentResults;

namespace BookingSystem.Domain.Restaurant.ValueObjects;

public sealed record DayOfWeekSchedule
{
    private DayOfWeekSchedule(){}
    public DayOfWeek DayOfWeek { get; private init; }
    public TimeOnly? OpeningTime { get; private init; } = null;
    public TimeOnly? ClosingTime { get; private init; } = null;
    public bool IsClosed { get; private init; }
    public bool IsAlwaysOpen => !IsClosed && OpeningTime == ClosingTime;
    public TimeSpan WorkTime => IsClosed ? TimeSpan.Zero : ClosingTime!.Value - OpeningTime!.Value;

    public static Result<DayOfWeekSchedule> Create(DayOfWeek dayOfWeek, TimeOnly? openingTime, TimeOnly? closingTime,
        bool isClosed)
    {
        if (isClosed && (openingTime is not null || closingTime is not null)
            || !isClosed && (openingTime is null || closingTime is null))
            return Result.Fail<DayOfWeekSchedule>(DayOfWeekScheduleErrors.AmbiguousSchedule);
        return new DayOfWeekSchedule()
        {
            DayOfWeek = dayOfWeek,
            OpeningTime = openingTime,
            ClosingTime = closingTime,
            IsClosed = isClosed
        };
    }

    public static DayOfWeekSchedule Create(DayOfWeek dayOfWeek, TimeOnly openingTime, TimeOnly closingTime)
        => new DayOfWeekSchedule()
        {
            DayOfWeek = dayOfWeek,
            OpeningTime = openingTime,
            ClosingTime = closingTime,
            IsClosed = false
        };
    public static DayOfWeekSchedule CreateDayOff(DayOfWeek dayOfWeek)
        => new DayOfWeekSchedule()
        {
            DayOfWeek = dayOfWeek,
            IsClosed = true
        };
}