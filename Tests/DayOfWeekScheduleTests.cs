using BookingSystem.Domain.Restaurant.ValueObjects;

namespace Tests;

public class DayOfWeekScheduleTests
{
    [Theory]
    [InlineData(9, 0, 17, 0, 8, 0)]
    [InlineData(23, 0, 3, 0, 4, 0)]
    [InlineData(12, 45, 21, 10, 8, 25)]
    public void WorkTime_WhenStartingTimeAndEndingTimeDiffer_ReturnsCorrectWorkTime(int startHour, int startMinute,
        int endHour, int endMinute, int expectedWorkTimeHours, int expectedWorkTimeMinutes)
    {
        var dows = DayOfWeekSchedule.Create(DayOfWeek.Monday, new TimeOnly(startHour, startMinute),
            new TimeOnly(endHour, endMinute), false);
        Assert.True(dows.IsSuccess);
        Assert.False(dows.Value.IsAlwaysOpen);
        Assert.Equal(new TimeSpan(expectedWorkTimeHours, expectedWorkTimeMinutes, 0), dows.Value.WorkTime);
    }

    [Fact]
    public void IsAlwaysOpen_WhenStartingTimeAndEndingTimeTheSame_ReturnsTrue()
    {
        var dows = DayOfWeekSchedule.Create(DayOfWeek.Thursday, new TimeOnly(13, 25), new TimeOnly(13, 25), false);
        Assert.True(dows.IsSuccess);
        Assert.True(dows.Value.IsAlwaysOpen);
    }
}