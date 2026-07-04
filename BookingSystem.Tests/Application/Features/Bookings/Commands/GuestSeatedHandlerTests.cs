using System.Linq.Expressions;
using BookingSystem.Application.Features.Bookings.Abstractions;
using BookingSystem.Application.Features.Bookings.Commands.Complete.CompleteBySystem;
using BookingSystem.Application.Features.Bookings.Commands.GuestSeated;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Users;
using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BookingSystem.Tests.Application.Features.Bookings.Commands;

public class GuestSeatedHandlerTests(PostgresTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    [Fact]
    public async Task When_BookingDoesNotExist_ShouldReturnNotFound()
    {
        var manager = await Users.CreateManagerAsync();
        SetCurrentUser(manager);
        var randomBookingId = Guid.NewGuid();

        NewScope();
        var res = await Mediator.Send(new GuestSeatedCommand(randomBookingId));
        res.IsFailed.Should().BeTrue();
        res.ShouldContain(BookingErrors.NotFound);
    }

    [Fact]
    public async Task When_UserIsNotRestaurantOwner_ReturnsAccessDeniedError()
    {
        var users = await Users.CreateBase3Async();
        var realManager = await Users.CreateManagerAsync("realManager12", "realMan23@gm.co", "+74561231245");
        User manager = users[1], guest = users[0];
        SetCurrentUser(manager);

        var restaurant = await Restaurants.CreateDefault(realManager.Id.Value);
        var booking = await Bookings.Create(builder =>
        {
            builder.WithGuest(guest)
                .WithRestaurant(restaurant)
                .WithGuestCount(2)
                .WithStatus(BookingStatus.Confirmed)
                .WithTableNumber(restaurant.Tables.First().TableNumber);
        });

        NewScope();
        var res = await Mediator.Send(new GuestSeatedCommand(booking.Id.Value));
        res.IsFailed.Should().BeTrue();
        res.ShouldContain(BookingErrors.AccessDenied);

        var bookingGuestSeated = await NewDbContext().Bookings.AsNoTracking()
            .Where(b => b.Status == BookingStatus.Seated).FirstOrDefaultAsync();
        bookingGuestSeated.Should().BeNull();
    }

    [Fact]
    public async Task When_UserIsRestaurantOwner_PassesAuthorization()
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[0];
        SetCurrentUser(manager);

        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(builder =>
        {
            builder.WithGuest(guest)
                .WithRestaurant(restaurant)
                .WithGuestCount(2)
                .WithStatus(BookingStatus.Confirmed)
                .WithTableNumber(restaurant.Tables.First().TableNumber);
        });

        NewScope();

        var res = await Mediator.Send(new GuestSeatedCommand(booking.Id.Value));
        res.ShouldNotContain(BookingErrors.AccessDenied);
    }

    [Fact]
    public async Task When_AvailabilityStateExpired_ReturnsInternalServerError()
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[0];
        SetCurrentUser(manager);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var booking = await Bookings.Create(builder =>
        {
            builder.WithGuest(guest)
                .WithRestaurant(restaurant)
                .WithGuestCount(2)
                .WithStatus(BookingStatus.Confirmed)
                .WithTableNumber(restaurant.Tables.First().TableNumber)
                .WithTimeSlotNoChecking(DateTimeOffset.UtcNow.AddDays(-1), TimeSpan.FromMinutes(90));
        });
        NewScope();
        var res = await Mediator.Send(new GuestSeatedCommand(booking.Id.Value));

        res.ShouldContain(BookingErrors.Expired);
        
        BackgroundJobServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task When_AvailabilityStateValid_UpdatesStatusOnly()
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[0];
        SetCurrentUser(manager);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);

        var scheduledAt = DateTimeOffset.UtcNow.AddSeconds(-5);
        var duration = TimeSpan.FromMinutes(90);
        var booking = await Bookings.Create(builder =>
        {
            builder.WithGuest(guest)
                .WithRestaurant(restaurant)
                .WithGuestCount(2)
                .WithStatus(BookingStatus.Confirmed)
                .WithTableNumber(restaurant.Tables.First().TableNumber)
                .WithTimeSlotNoChecking(scheduledAt, duration);
        });

        NewScope();
        var res = await Mediator.Send(new GuestSeatedCommand(booking.Id.Value));

        res.IsSuccess.Should().BeTrue();

        var updatedBooking = await NewDbContext().Bookings.AsNoTracking().SingleOrDefaultAsync();
        updatedBooking.Should().NotBeNull();
        updatedBooking.Status.Should().Be(BookingStatus.Seated);
        updatedBooking.TimeSlot.Start.Should().BeCloseTo(scheduledAt, TimeSpan.FromMilliseconds(5));
        updatedBooking.TimeSlot.End.Should().BeCloseTo(scheduledAt + duration, TimeSpan.FromMilliseconds(5));
        
        VerifyCompleteBookingBySystemCommand(booking);
    }

    private void VerifyCompleteBookingBySystemCommand(Booking booking)
    {
        BackgroundJobServiceMock.Verify(b => b.Schedule<IMediator>(
                It.Is<Expression<Action<IMediator>>>(e =>
                    MatchesCompleteBookingBySystemCommand(e, booking.Id.Value)),
                It.Is<DateTimeOffset>(dt =>
                    Math.Abs((dt - booking.TimeSlot.End).Ticks) < TimeSpan.TicksPerMillisecond)),
            Times.Once);
    }
    
    private static bool MatchesCompleteBookingBySystemCommand(Expression<Action<IMediator>> expression, Guid bookingId)
    {
        if (expression.Body is not MethodCallExpression methodCall) return false;
        if (methodCall.Method.Name != nameof(IMediator.Send)) return false;

        var argument = methodCall.Arguments[0];
        if (argument is not NewExpression newExpression) return false;
        if (newExpression.Constructor!.DeclaringType != typeof(CompleteBookingBySystemCommand)) return false;
        var id = Expression.Lambda<Func<Guid>>(newExpression.Arguments[0]).Compile()();
        if (id != bookingId) return false;
        return true;
    }

    [Fact]
    public async Task When_AvailabilityStateEarly_UpdatesStatusAndStartEndTime()
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[0];
        SetCurrentUser(manager);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);

        var scheduledAt = DateTimeOffset.UtcNow.AddMinutes(10);
        var duration = TimeSpan.FromMinutes(90);
        var booking = await Bookings.Create(builder =>
        {
            builder.WithGuest(guest)
                .WithRestaurant(restaurant)
                .WithGuestCount(2)
                .WithStatus(BookingStatus.Confirmed)
                .WithTableNumber(restaurant.Tables.First().TableNumber)
                .WithTimeSlotNoChecking(scheduledAt, duration);
        });

        NewScope();

        var res = await Mediator.Send(new GuestSeatedCommand(booking.Id.Value));
        res.IsSuccess.Should().BeTrue();

        var updatedBooking = await NewDbContext().Bookings.AsNoTracking().SingleOrDefaultAsync();
        updatedBooking.Should().NotBeNull();
        updatedBooking.Status.Should().Be(BookingStatus.Seated);
        updatedBooking.TimeSlot.Start.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        updatedBooking.TimeSlot.End.Should().BeCloseTo(DateTimeOffset.UtcNow + duration, TimeSpan.FromSeconds(5));
        
        VerifyCompleteBookingBySystemCommand(booking);
    }

    [Fact]
    public async Task When_AvailabilityStateEarly_ConflictingBookingExists_ReturnsTableNotAvailable()
    {
        var users = await Users.CreateBase3Async();
        User manager = users[1], guest = users[0];
        var guest2 = await Users.CreateGuestAsync("guest2", "guest2@example.com", "+76365257845");
        SetCurrentUser(manager);
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        var firstBooking = await Bookings.Create(c => c.WithGuest(guest2)
            .WithRestaurant(restaurant)
            .WithGuestCount(2)
            .WithStatus(BookingStatus.Confirmed)
            .WithTableNumber(restaurant.Tables.First().TableNumber)
            .WithTimeSlotNoChecking(DateTimeOffset.UtcNow.AddMinutes(15), TimeSpan.FromMinutes(90)));

        var scheduledAt = DateTimeOffset.UtcNow.AddMinutes(120);
        var duration = TimeSpan.FromMinutes(90);
        var booking = await Bookings.Create(builder =>
        {
            builder.WithGuest(guest)
                .WithRestaurant(restaurant)
                .WithGuestCount(2)
                .WithStatus(BookingStatus.Confirmed)
                .WithTableNumber(restaurant.Tables.First().TableNumber)
                .WithTimeSlotNoChecking(scheduledAt, duration);
        });

        NewScope();
        var res = await Mediator.Send(new GuestSeatedCommand(booking.Id.Value));
        res.ShouldContain(BookingErrors.TableNotAvailable);

        var updatedBookingCount = await NewDbContext().Bookings.AsNoTracking()
            .Where(b => b.Status == BookingStatus.Seated).CountAsync();
        updatedBookingCount.Should().Be(0);
        
        BackgroundJobServiceMock.VerifyNoOtherCalls();
    }
}