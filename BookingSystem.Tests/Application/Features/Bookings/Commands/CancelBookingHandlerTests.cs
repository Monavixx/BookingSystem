using BookingSystem.Application.Features.Bookings.Commands.Cancel;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Bookings.ValueObjects.Helpers;
using BookingSystem.Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Tests.Application.Features.Bookings.Commands;

public class CancelBookingHandlerTests(PostgresTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Valid_ShouldCancelBooking(int role)
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        SetCurrentUser(users[role]);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(builder => builder
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithGuestCount(2)
            .WithStatus(BookingStatus.Confirmed)
            .WithTableNumber(restaurant.Tables.First().TableNumber)
            .WithTimeSlotNoChecking(DateTimeOffset.UtcNow.AddHours(1), TimeSpan.FromMinutes(90)));
        NewScope();
        
        var res = await Mediator.Send(new CancelBookingCommand(booking.Id.Value));
        res.IsSuccess.Should().BeTrue();

        NewScope();
        var updatedBooking = await DbContext.Bookings.AsNoTracking().FirstAsync(b => b.Id == booking.Id);
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
            .WithTimeSlotNoChecking(DateTimeOffset.UtcNow.AddHours(1), TimeSpan.FromMinutes(90)));
        NewScope();
        
        SetCurrentUser(anotherGuest);
        var res = await Mediator.Send(new CancelBookingCommand(booking.Id.Value));
        res.ShouldContain(BookingErrors.AccessDenied);
        
        (await NewDbContext().Bookings.AsNoTracking().FirstAsync(b => b.Id == booking.Id))
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
            .WithTableNumber(restaurant.Tables.First().TableNumber)
            .WithTimeSlotNoChecking(DateTimeOffset.UtcNow.AddHours(1), TimeSpan.FromMinutes(90)));
        NewScope();
        
        SetCurrentUser(anotherManager);
        var res = await Mediator.Send(new CancelBookingCommand(booking.Id.Value));
        res.ShouldContain(BookingErrors.AccessDenied);
        
        (await NewDbContext().Bookings.AsNoTracking().FirstAsync(b => b.Id == booking.Id))
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
            .WithTableNumber(restaurant.Tables.First().TableNumber)
            .WithTimeSlotNoChecking(DateTimeOffset.UtcNow.AddHours(1), TimeSpan.FromMinutes(90)));
        NewScope();
        
        SetCurrentUser(guest);
        var res = await Mediator.Send(new CancelBookingCommand(booking.Id.Value));
        res.ShouldContain(BookingErrors.Status.InvalidStatusTransition);
        
        (await NewDbContext().Bookings.AsNoTracking().FirstAsync(b => b.Id == booking.Id))
            .Status.Should().Be(finalStatus);
    }
    
    [Fact]
    public async Task When_BookingDoesNotExist_ShouldReturnNotFound()
    {
        var users = await Users.CreateBase3Async();
        SetCurrentUser(users[2]);
        var res = await Mediator.Send(new CancelBookingCommand(Guid.NewGuid()));
        res.ShouldContain(BookingErrors.NotFound);
    }
    
    [Fact]
    public async Task Then_GuestSeated_ShouldReturnInvalidStatusTransition()
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[2];
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(builder => builder
            .WithGuest(guest)
            .WithRestaurant(restaurant)
            .WithGuestCount(2)
            .WithStatus(BookingStatus.Seated)
            .WithTableNumber(restaurant.Tables.First().TableNumber)
            .WithTimeSlotNoChecking(DateTimeOffset.UtcNow.AddHours(1), TimeSpan.FromMinutes(90)));
        NewScope();
        
        SetCurrentUser(guest);
        var res = await Mediator.Send(new CancelBookingCommand(booking.Id.Value));
        res.ShouldContain(BookingErrors.Status.InvalidStatusTransition);
        
        (await NewDbContext().Bookings.AsNoTracking().FirstAsync(b => b.Id == booking.Id))
            .Status.Should().Be(BookingStatus.Seated);
    }
}