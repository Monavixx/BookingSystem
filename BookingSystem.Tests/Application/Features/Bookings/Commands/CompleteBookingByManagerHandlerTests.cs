using BookingSystem.Application.Features.Bookings.Commands.Complete;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Tests.Application.Features.Bookings.Commands;

public class CompleteBookingByManagerHandlerTests(IntegrationTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    [Fact]
    public async Task When_Manager_BookingStatusSeated_ShouldCompleteBooking()
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        SetReadOnlyCurrentUser(manager);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithStatus(BookingStatus.Seated));
        NewScope();

        var res = await Mediator.Send(
            new CompleteBookingCommand(booking.Id.Value),
            TestContext.Current.CancellationToken);

        res.IsSuccess.Should().BeTrue();
        NewScope();

        var updatedBooking = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        updatedBooking.Status.Should().Be(BookingStatus.Completed);
    }

    [Fact]
    public async Task When_BookingDoesNotExist_ShouldReturnNotFound()
    {
        var manager = await Users.CreateManagerAsync();
        SetReadOnlyCurrentUser(manager);
        NewScope();

        var res = await Mediator.Send(
            new CompleteBookingCommand(Guid.NewGuid()),
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
            .WithStatus(BookingStatus.Seated));
        SetReadOnlyCurrentUser(anotherManager);
        NewScope();

        var res = await Mediator.Send(
            new CompleteBookingCommand(booking.Id.Value),
            TestContext.Current.CancellationToken);

        res.ShouldContain(BookingErrors.AccessDenied);
        NewScope();

        var updatedBooking = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == booking.Id, cancellationToken: TestContext.Current.CancellationToken);
        updatedBooking.Status.Should().Be(BookingStatus.Seated);
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.ConfirmedByGuest)]
    [InlineData(BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Canceled)]
    [InlineData(BookingStatus.Completed)]
    public async Task When_BookingNotInSeatedStatus_ShouldReturnInvalidStatusTransition(BookingStatus status)
    {
        // Arrange
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        SetReadOnlyCurrentUser(manager);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithStatus(status));
        NewScope();

        // Act
        var res = await Mediator.Send(
            new CompleteBookingCommand(booking.Id.Value),
            TestContext.Current.CancellationToken);

        // Assert
        res.IsFailed.Should().BeTrue();
        res.Errors.Should().NotBeEmpty();
        NewScope();

        var updatedBooking = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        updatedBooking.Status.Should().Be(status);
    }

    [Fact]
    public async Task When_Guest_AttemptsToCompleteBooking_ShouldReturnAccessDenied()
    {
        // Arrange
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithStatus(BookingStatus.Seated));
        SetReadOnlyCurrentUser(guest);
        NewScope();

        // Act
        var res = await Mediator.Send(
            new CompleteBookingCommand(booking.Id.Value),
            TestContext.Current.CancellationToken);

        // Assert
        res.ShouldContain(BookingErrors.AccessDenied);
        NewScope();

        var updatedBooking = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == booking.Id, cancellationToken: TestContext.Current.CancellationToken);
        updatedBooking.Status.Should().Be(BookingStatus.Seated);
    }

    [Fact]
    public async Task When_Admin_AttemptsToCompleteBooking_ShouldReturnAccessDenied()
    {
        // Arrange
        var (admin, manager, guest, _) = await Users.CreateBase4Async();
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithStatus(BookingStatus.Seated));
        SetReadOnlyCurrentUser(admin);
        NewScope();

        // Act
        var res = await Mediator.Send(
            new CompleteBookingCommand(booking.Id.Value),
            TestContext.Current.CancellationToken);

        // Assert
        res.ShouldContain(BookingErrors.AccessDenied);
        NewScope();

        var updatedBooking = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == booking.Id, cancellationToken: TestContext.Current.CancellationToken);
        updatedBooking.Status.Should().Be(BookingStatus.Seated);
    }

    [Fact]
    public async Task When_MultipleBookingsFromSameRestaurant_ShouldCompleteOnlySpecifiedBooking()
    {
        // Arrange
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        SetReadOnlyCurrentUser(manager);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);

        var booking1 = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithStatus(BookingStatus.Seated));
        var booking2 = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithStatus(BookingStatus.Seated)
            .WithTableNumber(restaurant.Tables.Last().TableNumber));
        NewScope();

        // Act
        var res = await Mediator.Send(
            new CompleteBookingCommand(booking1.Id.Value),
            TestContext.Current.CancellationToken);

        // Assert
        res.IsSuccess.Should().BeTrue();
        NewScope();

        var updatedBooking1 = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == booking1.Id, cancellationToken: TestContext.Current.CancellationToken);
        var updatedBooking2 = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == booking2.Id, cancellationToken: TestContext.Current.CancellationToken);

        updatedBooking1.Status.Should().Be(BookingStatus.Completed);
        updatedBooking2.Status.Should().Be(BookingStatus.Seated);
    }

    [Fact]
    public async Task When_ManagerWithMultipleRestaurants_ShouldCompleteFromCorrectRestaurant()
    {
        // Arrange
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
            .WithStatus(BookingStatus.Seated));
        var booking2 = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurants[1])
            .WithStatus(BookingStatus.Seated));
        NewScope();

        // Act
        var res = await Mediator.Send(
            new CompleteBookingCommand(booking1.Id.Value),
            TestContext.Current.CancellationToken);

        // Assert
        res.IsSuccess.Should().BeTrue();
        NewScope();

        var updatedBooking1 = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == booking1.Id, cancellationToken: TestContext.Current.CancellationToken);
        var updatedBooking2 = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == booking2.Id, cancellationToken: TestContext.Current.CancellationToken);

        updatedBooking1.Status.Should().Be(BookingStatus.Completed);
        updatedBooking2.Status.Should().Be(BookingStatus.Seated);
    }

    [Fact]
    public async Task When_Persist_ChangesAreSavedToDatabase()
    {
        // Arrange
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        SetReadOnlyCurrentUser(manager);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(c => c
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithStatus(BookingStatus.Seated));
        var bookingId = booking.Id;
        NewScope();

        // Act
        var res = await Mediator.Send(
            new CompleteBookingCommand(bookingId.Value),
            TestContext.Current.CancellationToken);

        // Assert
        res.IsSuccess.Should().BeTrue();

        // Verify with a completely new scope/context
        NewScope();
        var dbBooking = await DbContext.Bookings.AsNoTracking()
            .SingleAsync(b => b.Id == bookingId, cancellationToken: TestContext.Current.CancellationToken);

        dbBooking.Status.Should().Be(BookingStatus.Completed);
    }
}
