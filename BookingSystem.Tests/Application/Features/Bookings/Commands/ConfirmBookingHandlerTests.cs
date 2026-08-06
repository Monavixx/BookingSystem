using BookingSystem.Application.Features.Bookings.Commands.Confirm;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Tests.Application.Features.Bookings.Commands;

public class ConfirmBookingHandlerTests(PostgresTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    [Fact]
    public async Task When_ManagerConfirmsBookingInConfirmedByGuestStatus_ShouldConfirmBooking()
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        SetReadOnlyCurrentUser(manager);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithStatus(BookingStatus.ConfirmedByGuest));
        NewScope();

        var res = await Mediator.Send(new ConfirmBookingCommand(booking.Id.Value), TestContext.Current.CancellationToken);
        res.IsSuccess.Should().BeTrue();
        NewScope();

        var updatedBooking = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        updatedBooking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task When_BookingDoesNotExist_ShouldReturnNotFound()
    {
        var manager = await Users.CreateManagerAsync();
        SetReadOnlyCurrentUser(manager);
        NewScope();

        var res = await Mediator.Send(
            new ConfirmBookingCommand(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        res.ShouldContain(BookingErrors.NotFound);
    }

    [Fact]
    public async Task When_ManagerOfAnotherRestaurant_ShouldReturnAccessDenied()
    {
        var (_, manager, anotherManager, guest, _) = await Users.CreateBase5Async();
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithStatus(BookingStatus.ConfirmedByGuest));
        SetReadOnlyCurrentUser(anotherManager);
        NewScope();

        var res = await Mediator.Send(
            new ConfirmBookingCommand(booking.Id.Value),
            TestContext.Current.CancellationToken);

        res.ShouldContain(BookingErrors.AccessDenied);
        NewScope();

        var updatedBooking = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == booking.Id, cancellationToken: TestContext.Current.CancellationToken);
        updatedBooking.Status.Should().Be(BookingStatus.ConfirmedByGuest);
    }

    [Fact]
    public async Task When_GuestAttemptsToConfirm_ShouldReturnAccessDenied()
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithStatus(BookingStatus.ConfirmedByGuest));
        SetReadOnlyCurrentUser(guest);
        NewScope();

        var res = await Mediator.Send(
            new ConfirmBookingCommand(booking.Id.Value),
            TestContext.Current.CancellationToken);

        res.ShouldContain(BookingErrors.AccessDenied);
        NewScope();

        var updatedBooking = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == booking.Id, cancellationToken: TestContext.Current.CancellationToken);
        updatedBooking.Status.Should().Be(BookingStatus.ConfirmedByGuest);
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Seated)]
    [InlineData(BookingStatus.Completed)]
    [InlineData(BookingStatus.Canceled)]
    public async Task When_BookingNotInConfirmedByGuestStatus_ShouldReturnInvalidStatusTransition(BookingStatus status)
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        SetReadOnlyCurrentUser(manager);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithStatus(status));
        NewScope();

        var res = await Mediator.Send(
            new ConfirmBookingCommand(booking.Id.Value),
            TestContext.Current.CancellationToken);

        res.IsFailed.Should().BeTrue();
        res.Errors.Should().NotBeEmpty();
        NewScope();

        var updatedBooking = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        updatedBooking.Status.Should().Be(status);
    }

    [Fact]
    public async Task When_MultipleBookingsFromSameRestaurant_ShouldConfirmOnlySpecifiedBooking()
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        SetReadOnlyCurrentUser(manager);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);

        var booking1 = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithStatus(BookingStatus.ConfirmedByGuest));
        var booking2 = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithStatus(BookingStatus.ConfirmedByGuest)
            .WithTableNumber(restaurant.Tables.Last().TableNumber));
        NewScope();

        var res = await Mediator.Send(
            new ConfirmBookingCommand(booking1.Id.Value),
            TestContext.Current.CancellationToken);

        res.IsSuccess.Should().BeTrue();
        NewScope();

        var updatedBooking1 = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == booking1.Id, cancellationToken: TestContext.Current.CancellationToken);
        var updatedBooking2 = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == booking2.Id, cancellationToken: TestContext.Current.CancellationToken);

        updatedBooking1.Status.Should().Be(BookingStatus.Confirmed);
        updatedBooking2.Status.Should().Be(BookingStatus.ConfirmedByGuest);
    }

    [Fact]
    public async Task When_ManagerWithMultipleRestaurants_ShouldConfirmFromCorrectRestaurant()
    {
        var users = await Users.CreateBase5Async();
        User manager = users.Manager, guest = users.Guest;
        var restaurants = await Restaurants.CreateRestaurants(c =>
        {
            c.AddRestaurant(manager, (1, 4), (2, 4), (3, 20))
                .AddRestaurant(manager, (1, 2), (2, 3), (3, 8), (4, 3));
        });

        SetReadOnlyCurrentUser(manager);
        var booking1 = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurants[0])
            .WithStatus(BookingStatus.ConfirmedByGuest));
        var booking2 = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurants[1])
            .WithStatus(BookingStatus.ConfirmedByGuest));
        NewScope();

        var res = await Mediator.Send(
            new ConfirmBookingCommand(booking1.Id.Value),
            TestContext.Current.CancellationToken);

        res.IsSuccess.Should().BeTrue();
        NewScope();

        var updatedBooking1 = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == booking1.Id, cancellationToken: TestContext.Current.CancellationToken);
        var updatedBooking2 = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == booking2.Id, cancellationToken: TestContext.Current.CancellationToken);

        updatedBooking1.Status.Should().Be(BookingStatus.Confirmed);
        updatedBooking2.Status.Should().Be(BookingStatus.ConfirmedByGuest);
    }

    [Fact]
    public async Task When_HappyPath_PersistChangesAreSavedToDatabase()
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        SetReadOnlyCurrentUser(manager);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithStatus(BookingStatus.ConfirmedByGuest));
        var bookingId = booking.Id;
        NewScope();

        var res = await Mediator.Send(
            new ConfirmBookingCommand(bookingId.Value),
            TestContext.Current.CancellationToken);

        res.IsSuccess.Should().BeTrue();

        NewScope();
        var dbBooking = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == bookingId, cancellationToken: TestContext.Current.CancellationToken);

        dbBooking.Status.Should().Be(BookingStatus.Confirmed);
    }
}

