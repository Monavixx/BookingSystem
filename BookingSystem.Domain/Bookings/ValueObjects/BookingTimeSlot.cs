using BookingSystem.Domain.Bookings.Errors;
using FluentResults;

namespace BookingSystem.Domain.Bookings.ValueObjects;

public sealed record BookingTimeSlot
{
    public DateTimeOffset Start { get; private init; }
    public DateTimeOffset End { get; private init; }
    private BookingTimeSlot(){}

    public static Result<BookingTimeSlot> Create(DateTimeOffset start, DateTimeOffset end)
    {
        if (end < start) return Result.Fail<BookingTimeSlot>(BookingErrors.TimeSlot.InvalidTimeRange);
        return new BookingTimeSlot() { Start = start, End = end };
    }

    internal static BookingTimeSlot __CreateWithNoChecking(DateTimeOffset start, DateTimeOffset end) =>
        new BookingTimeSlot() { Start = start, End = end };
}