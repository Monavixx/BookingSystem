using BookingSystem.Application.Features.Bookings.Abstractions;
using BookingSystem.Application.Features.Bookings.Commands.ConfirmByGuest;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Tests.Application.Features.Bookings.Commands;

public class ConfirmBookingByGuestHandlerTests(PostgresTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    [Fact]
    public async Task When_Guest_BookingStatusPending_ShouldConfirmByGuest()
    {
        var users = await Users.CreateBase3Async();
        User guest = users[2], manager = users[1];
        SetCurrentUser(guest);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant));
        NewScope();

        var res = await Mediator.Send(new ConfirmBookingByGuestCommand(booking.Id.Value), TestContext.Current.CancellationToken);
        res.IsSuccess.Should().BeTrue();
        BackgroundJobServiceMock.Verify(b => b.Schedule<IBookingCancellationService>(
            s => s.CancelAsync(booking.Id, CancellationReason.ManagerHasNotConfirmed),
            booking.TimeSlot.Start));
        NewScope();

        var updatedBooking = await DbContext.Bookings.AsNoTracking().SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        updatedBooking.Status.Should().Be(BookingStatus.ConfirmedByGuest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task When_NotBooker_BookingStatusPending_ShouldReturnAccessDenied(int user)
    {
        var users = await Users.CreateBase3Async();
        var manager = users[1];
        var anotherGuest = await Users.CreateGuestAsync(
            "popopo", "po@g.com", "+79468751245");
        SetCurrentUser(users[user]);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(c => c
            .WithGuest(anotherGuest)
            .WithRestaurant(restaurant));
        NewScope();

        var res = await Mediator.Send(new ConfirmBookingByGuestCommand(booking.Id.Value), TestContext.Current.CancellationToken);
        res.ShouldContain(BookingErrors.AccessDenied);
        NewScope();

        var updatedBooking = await DbContext.Bookings.AsNoTracking().SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        updatedBooking.Status.Should().Be(BookingStatus.Pending);
    }
}