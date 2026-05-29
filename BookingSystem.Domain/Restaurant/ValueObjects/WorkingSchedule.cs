using BookingSystem.Domain.Restaurant.Errors;
using FluentResults;

namespace BookingSystem.Domain.Restaurant.ValueObjects;

public sealed class WorkingSchedule
{
    private List<DayOfWeekSchedule> _dayOfWeekSchedules = [];
    public IReadOnlyCollection<DayOfWeekSchedule> DayOfWeekSchedules => _dayOfWeekSchedules;
    
    private WorkingSchedule(){}
    
    public static Result<WorkingSchedule> Create(IEnumerable<DayOfWeekSchedule> dayOfWeekSchedules)
    {
        var schedules = dayOfWeekSchedules.ToList();
        if(schedules.Count != 7 || schedules.DistinctBy(s=>s.DayOfWeek).Count() != 7)
            return Result.Fail<WorkingSchedule>(WorkingScheduleErrors.DaysOutOfRange);
        return new WorkingSchedule
        {
            _dayOfWeekSchedules = schedules
        };
    }

    public DayOfWeekSchedule GetDayOfWeekSchedule(DayOfWeek dayOfWeek)
        => _dayOfWeekSchedules.First(d => d.DayOfWeek == dayOfWeek);
}