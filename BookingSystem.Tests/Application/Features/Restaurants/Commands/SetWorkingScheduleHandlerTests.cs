using BookingSystem.Application.Features.Restaurants.Commands.SetWorkingSchedule;
using BookingSystem.Application.Features.Restaurants.DTOs;
using BookingSystem.Domain.Restaurants;
using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Tests.Application.Features.Restaurants.Commands;

public class SetWorkingScheduleHandlerTests(PostgresTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    private async Task<(User manager, Restaurant restaurant)> CreateManagerAndRestaurant()
    {
        var manager = await Users.CreateManagerAsync();
        return (manager, await Restaurants.CreateDefault(manager.Id.Value));
    }

    [Fact]
    public async Task HappyPath_And_PreviousWorkingScheduleWasNull_ShouldSetWorkingSchedule()
    {
        var (manager, restaurant) = await CreateManagerAndRestaurant();
        SetCurrentUser(manager);

        var res = await Mediator.Send(new SetWorkingScheduleCommand(restaurant.Id.Value, [
            new DayOfWeekScheduleRequest(DayOfWeek.Monday, TimeOnly.FromTimeSpan(new TimeSpan(9, 0, 0)),
                TimeOnly.FromTimeSpan(new TimeSpan(17, 0, 0)), false),
            new DayOfWeekScheduleRequest(DayOfWeek.Tuesday, TimeOnly.FromTimeSpan(new TimeSpan(9, 0, 0)),
                TimeOnly.FromTimeSpan(new TimeSpan(17, 0, 0)), false),
            new DayOfWeekScheduleRequest(null, TimeOnly.FromTimeSpan(new TimeSpan(9, 0, 0)),
                TimeOnly.FromTimeSpan(new TimeSpan(17, 0, 0)), false),
            new DayOfWeekScheduleRequest(null, TimeOnly.FromTimeSpan(new TimeSpan(9, 0, 0)),
                TimeOnly.FromTimeSpan(new TimeSpan(17, 0, 0)), false),
            new DayOfWeekScheduleRequest(null, TimeOnly.FromTimeSpan(new TimeSpan(9, 0, 0)),
                TimeOnly.FromTimeSpan(new TimeSpan(17, 0, 0)), false),
            new DayOfWeekScheduleRequest(null, null, null, true),
            new DayOfWeekScheduleRequest(null, null, null, true)
        ]), TestContext.Current.CancellationToken);
        res.Errors.Should().BeEmpty();
        NewScope();

        var schedulesList = await DbContext.Database.SqlQuery<DayOfWeekSchedule>(
            $"""
             SELECT * FROM restaurant_daily_schedules
             WHERE restaurant_id = {restaurant.Id.Value}
             """).ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        schedulesList.Should().HaveCount(7);
        schedulesList.DistinctBy(d => d.DayOfWeek).Should().HaveCount(7);
    }
}