namespace BookingSystem.Application.Features.Restaurants.DTOs;

public class DayOfWeekScheduleRequest
{
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeOnly? OpeningTime { get; set; }
    public TimeOnly? ClosingTime { get; set; }
    public bool? IsClosed { get; set; }
}