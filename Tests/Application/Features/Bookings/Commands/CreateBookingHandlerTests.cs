using BookingSystem.Application.Features.Bookings.Commands.Create;
using BookingSystem.Domain.Bookings.Services;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Application.Features.Bookings.Commands;

public class CreateBookingHandlerTests(PostgresTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    [Theory]
    [InlineData(1)]
    [InlineData(null)]
    public async Task When_ScheduledInTheFuture_And_TableIsAvailable_ShouldCreateBooking(int? tableNumber)
    {
        var users = await Users.CreateBase3Async();
        User guest = users[2], manager = users[1];
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        CurrentUserService.UserIdGuid = guest.Id.Value;

        var hourLater = DateTimeOffset.UtcNow.AddHours(1);
        var res = await Mediator.Send(
            new CreateBookingCommand(
                GuestId: guest.Id.Value,
                GuestCount: 2,
                RestaurantId: restaurant.Id.Value,
                TableNumber: tableNumber,
                ScheduledAt: hourLater));
        res.Errors.Should().BeEmpty();
        
        var booking = await DbContext.Bookings.SingleOrDefaultAsync();
        booking.Should().NotBeNull();
        booking.TableNumber.Should().Be(1);
        booking.Status.Should().Be(BookingStatus.Pending);
        booking.TimeSlot.Start.Should().Be(hourLater);
        booking.TimeSlot.End.Should()
            .Be(hourLater + Scope.ServiceProvider.GetRequiredService<BookingDurationCalculator>()
                .CalculateDuration(2).Value);
    }
}