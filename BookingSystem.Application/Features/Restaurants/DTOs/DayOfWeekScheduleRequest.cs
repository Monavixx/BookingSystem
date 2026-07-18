namespace BookingSystem.Application.Features.Restaurants.DTOs;

public sealed record DayOfWeekScheduleRequest(
    DayOfWeek? DayOfWeek,
    TimeOnly? OpeningTime,
    TimeOnly? ClosingTime,
    bool? IsClosed);