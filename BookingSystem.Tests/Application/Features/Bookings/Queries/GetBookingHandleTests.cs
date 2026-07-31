using BookingSystem.Application.Features.Bookings.Queries.Get;
using BookingSystem.Domain.Bookings.ValueObjects;
using FluentAssertions;

namespace BookingSystem.Tests.Application.Features.Bookings.Queries;

public class GetBookingHandleTests(PostgresTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task When_TheGuestRequests_ReturnBooking()
    {
        (BookingSystem.Domain.Users.User Admin, BookingSystem.Domain.Users.User Manager, BookingSystem.Domain.Users.User Guest, BookingSystem.Domain.Users.User AnotherGuest) = await Users.CreateBase4Async();
        var restaurant = await Restaurants.CreateDefault(Manager.Id.Value);
        var booking = await Bookings.Create(b => b
                .WithGuest(Guest)
                .WithRestaurant(restaurant)
                .WithStatus(BookingStatus.Confirmed));
        NewScope();

        SetCurrentUser(Guest);
        var res = await Mediator.Send(new GetBookingQuery(booking.Id.Value), TestContext.Current.CancellationToken);
        res.ShouldBeSuccess();
        res.Value.Id.Should().Be(booking.Id.Value);
    }
}
