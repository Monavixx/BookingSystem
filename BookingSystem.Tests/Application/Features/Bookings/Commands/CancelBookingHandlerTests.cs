using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Features.Bookings.Commands.Cancel;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Bookings.ValueObjects.Helpers;
using BookingSystem.Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Tests.Application.Features.Bookings.Commands;

public class CancelBookingHandlerTests(IntegrationTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Valid_ShouldCancelBooking(int role)
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        SetReadOnlyCurrentUser(users[role]);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(builder => builder
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithGuestCount(2)
            .WithStatus(BookingStatus.Confirmed)
            .WithTableNumber(restaurant.Tables.First().TableNumber));
        NewScope();

        var res = await Mediator.Send(new CancelBookingCommand(booking.Id.Value), TestContext.Current.CancellationToken);
        res.IsSuccess.Should().BeTrue();

        NewScope();
        var updatedBooking = await DbContext.Bookings.AsNoTracking().FirstAsync(b => b.Id == booking.Id, cancellationToken: TestContext.Current.CancellationToken);
        updatedBooking.Status.Should().Be(BookingStatus.Canceled);
    }

    [Fact]
    public async Task When_AnotherGuest_ShouldReturnAccessDenied()
    {
        var (_, manager, guest, anotherGuest) = await Users.CreateBase4Async();
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(builder => builder
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithGuestCount(2)
            .WithStatus(BookingStatus.Confirmed)
            .WithTableNumber(restaurant.Tables.First().TableNumber)
            .WithTimeSlotNoChecking(FakeTime.GetUtcNow().AddHours(1), TimeSpan.FromMinutes(90)));
        NewScope();

        SetReadOnlyCurrentUser(anotherGuest);
        var res = await Mediator.Send(new CancelBookingCommand(booking.Id.Value), TestContext.Current.CancellationToken);
        res.ShouldContain(BookingErrors.AccessDenied);

        (await NewDbContext().Bookings.AsNoTracking().FirstAsync(b => b.Id == booking.Id, cancellationToken: TestContext.Current.CancellationToken))
            .Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task When_ManagerOfAnotherRestaurant_ShouldReturnAccessDenied()
    {
        var (_, manager, anotherManager, guest, _) = await Users.CreateBase5Async();
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(builder => builder
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithGuestCount(2)
            .WithStatus(BookingStatus.Confirmed)
            .WithTableNumber(restaurant.Tables.First().TableNumber));
        NewScope();

        SetReadOnlyCurrentUser(anotherManager);
        var res = await Mediator.Send(new CancelBookingCommand(booking.Id.Value), TestContext.Current.CancellationToken);
        res.ShouldContain(BookingErrors.AccessDenied);

        (await NewDbContext().Bookings.AsNoTracking().FirstAsync(b => b.Id == booking.Id, cancellationToken: TestContext.Current.CancellationToken))
            .Status.Should().Be(BookingStatus.Confirmed);
    }

    public static TheoryData<BookingStatus> FinalStatuses =>
        new TheoryData<BookingStatus>(BookingStatusHelper.FinalStatuses);

    [Theory, MemberData(nameof(BookingStatusHelper.FinalStatuses))]
    public async Task When_BookingHasFinalStatus_ShouldReturnInvalidStatusTransition(BookingStatus finalStatus)
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(builder => builder
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithGuestCount(2)
            .WithStatus(finalStatus)
            .WithTableNumber(restaurant.Tables.First().TableNumber));
        NewScope();

        SetReadOnlyCurrentUser(guest);
        var res = await Mediator.Send(new CancelBookingCommand(booking.Id.Value), TestContext.Current.CancellationToken);
        res.ShouldContain(BookingErrors.Status.InvalidStatusOrReasonToCancelCode);

        (await NewDbContext().Bookings.AsNoTracking().FirstAsync(b => b.Id == booking.Id, cancellationToken: TestContext.Current.CancellationToken))
            .Status.Should().Be(finalStatus);
    }

    [Fact]
    public async Task When_BookingDoesNotExist_ShouldReturnNotFound()
    {
        var users = await Users.CreateBase3Async();
        SetReadOnlyCurrentUser(users[2]);
        var res = await Mediator.Send(new CancelBookingCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);
        res.ShouldContain(BookingErrors.NotFound);
    }

    [Fact]
    public async Task When_GuestSeated_ShouldReturnInvalidStatusTransition()
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(builder => builder
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithGuestCount(2)
            .WithStatus(BookingStatus.Seated)
            .WithTableNumber(restaurant.Tables.First().TableNumber));
        NewScope();

        SetReadOnlyCurrentUser(guest);
        var res = await Mediator.Send(new CancelBookingCommand(booking.Id.Value), TestContext.Current.CancellationToken);
        res.ShouldContain(BookingErrors.Status.InvalidStatusOrReasonToCancelCode);

        (await NewDbContext().Bookings.AsNoTracking().FirstAsync(b => b.Id == booking.Id, cancellationToken: TestContext.Current.CancellationToken))
            .Status.Should().Be(BookingStatus.Seated);
    }

    [Fact]
    public async Task When_Guest_SuccessfullyCancels_ShouldCreateCancellationRecord()
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        SetReadOnlyCurrentUser(guest);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(builder => builder
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithGuestCount(2)
            .WithStatus(BookingStatus.Confirmed)
            .WithTableNumber(restaurant.Tables.First().TableNumber));
        NewScope();

        var res = await Mediator.Send(new CancelBookingCommand(booking.Id.Value), TestContext.Current.CancellationToken);
        res.IsSuccess.Should().BeTrue();

        NewScope();
        var cr = await DbContext.CancellationRecords.AsNoTracking().SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        cr.WhoCancelledId.Should().Be(guest.Id);
        cr.CanceledAt.Should().BeCloseTo(FakeTime.GetUtcNow(), TimeSpan.FromMilliseconds(1));
        cr.BookingId.Should().Be(booking.Id);

        BackgroundJobServiceMock.Verify(b => b.Enqueue<IUserBlocker>(
            u => u.BlockUserIfCancellationPolicyViolated(booking.GuestId)));
    }

    [Fact]
    public async Task When_Manager_SuccessfullyCancelsBookingOfAnotherGuestByTheirRequest_ShouldCreateValidCancellationRecord()
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        SetReadOnlyCurrentUser(manager);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(builder => builder
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithGuestCount(2)
            .WithStatus(BookingStatus.Confirmed)
            .WithTableNumber(restaurant.Tables.First().TableNumber));
        NewScope();

        var res = await Mediator.Send(new CancelBookingCommand(booking.Id.Value, true), TestContext.Current.CancellationToken);
        res.IsSuccess.Should().BeTrue();

        NewScope();
        var cr = await DbContext.CancellationRecords.AsNoTracking().SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        cr.WhoCancelledId.Should().Be(manager.Id);
        cr.CanceledAt.Should().BeCloseTo(FakeTime.GetUtcNow(), TimeSpan.FromMilliseconds(1));
        cr.BookingId.Should().Be(booking.Id);

        BackgroundJobServiceMock.Verify(b => b.Enqueue<IUserBlocker>(
            u => u.BlockUserIfCancellationPolicyViolated(booking.GuestId)));
    }
}
