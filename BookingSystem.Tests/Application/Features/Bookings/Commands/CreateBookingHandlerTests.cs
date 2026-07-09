using System.Linq.Expressions;
using BookingSystem.Application.Features.Bookings.Abstractions;
using BookingSystem.Application.Features.Bookings.Commands.Create;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.Services;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Restaurants.Errors;
using BookingSystem.Domain.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BookingSystem.Tests.Application.Features.Bookings.Commands;

public class CreateBookingHandlerTests(PostgresTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    private void ShouldScheduleBookingStatusChangeJobWithTimeout(Booking booking)
    {
        BackgroundJobServiceMock.Verify(service => service.Schedule<IBookingCancellationService>(
            s=>s.CancelAsync(booking.Id, CancellationReason.PendingTimeout),
            It.Is<TimeSpan>(t => t > TimeSpan.Zero)), Times.Once);
    }
    
    // private void ShouldScheduleBookingStatusChangeJobAtStartTime(Booking booking)
    // {
    //     BackgroundJobServiceMock.Verify(service => service.Schedule<IBookingCancellationService>(
    //         s=>s.CancelIfNotConfirmedAsync(booking.Id),
    //         It.Is<DateTimeOffset>(t => Math.Abs((t - booking.TimeSlot.Start).Ticks) < TimeSpan.TicksPerMillisecond)), Times.Once);
    // }
    
    [Theory]
    [InlineData(1)]
    [InlineData(null)]
    public async Task When_ScheduledInTheFuture_And_TableIsAvailable_ShouldCreateBooking(int? tableNumber)
    {
        var users = await Users.CreateBase3Async();
        User guest = users[2], manager = users[1];
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        SetCurrentUser(guest);

        var scheduledAt = FakeTime.GetUtcNow().AddHours(1);
        var res = await Mediator.Send(
            new CreateBookingCommand(
                GuestCount: 2,
                RestaurantId: restaurant.Id.Value,
                TableNumber: tableNumber,
                ScheduledAt: scheduledAt));
        res.Errors.Should().BeEmpty();
        
        var booking = await DbContext.Bookings.SingleOrDefaultAsync();
        booking.Should().NotBeNull();
        booking.TableNumber.Should().Be(1);
        booking.Status.Should().Be(BookingStatus.Pending);
        booking.TimeSlot.Start.Should().BeCloseTo(scheduledAt, TimeSpan.FromSeconds(1));
        booking.TimeSlot.End.Should()
            .BeCloseTo(scheduledAt + Scope.ServiceProvider.GetRequiredService<BookingDurationCalculator>()
                .CalculateDuration(2).Value, TimeSpan.FromSeconds(1));
        
        ShouldScheduleBookingStatusChangeJobWithTimeout(booking);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(null)]
    public async Task When_ScheduledInTheFuture_And_NoTableHasAppropriateCapacity_ShouldNotCreateBooking(int? tableNumber)
    {
        var users = await Users.CreateBase3Async();
        User guest = users[2], manager = users[1];
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        SetCurrentUser(guest);

        var scheduledAt = FakeTime.GetUtcNow().AddHours(1);
        var res = await Mediator.Send(
            new CreateBookingCommand(
                GuestCount: 5,
                RestaurantId: restaurant.Id.Value,
                TableNumber: tableNumber,
                ScheduledAt: scheduledAt));
        res.IsFailed.Should().BeTrue();
        
        var booking = await DbContext.Bookings.SingleOrDefaultAsync();
        booking.Should().BeNull();
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(null)]
    public async Task When_ScheduledInThePast_ShouldNotCreateBooking(int? tableNumber)
    {
        var users = await Users.CreateBase3Async();
        User guest = users[2], manager = users[1];
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        SetCurrentUser(guest);

        var scheduledAt = FakeTime.GetUtcNow().AddHours(-1);

        NewScope();
        var res = await Mediator.Send(
            new CreateBookingCommand(
                GuestCount: 2,
                RestaurantId: restaurant.Id.Value,
                TableNumber: tableNumber,
                ScheduledAt: scheduledAt));
        res.IsFailed.Should().BeTrue();
        
        var booking = await NewDbContext().Bookings.SingleOrDefaultAsync();
        booking.Should().BeNull();
        BackgroundJobServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task When_AllAppropriateTablesAreOccupied_ShouldNotCreateBooking()
    {
        var users = await Users.CreateBase3Async();
        User guest = users[2], manager = users[1];
        var guest2 = await Users.CreateGuestAsync("bimba", "bimba@gmail.com", "+77777778899");
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        SetCurrentUser(guest);

        var scheduledAt = FakeTime.GetUtcNow().AddHours(1);

        NewScope();
        // Create a booking for the only table with capacity 2
        var res1 = await Mediator.Send(
            new CreateBookingCommand(
                GuestCount: 2,
                RestaurantId: restaurant.Id.Value,
                TableNumber: null,
                ScheduledAt: scheduledAt));
        res1.IsSuccess.Should().BeTrue();

        var firstBooking = await NewDbContext().Bookings.AsNoTracking().SingleOrDefaultAsync();
        firstBooking.Should().NotBeNull();
        ShouldScheduleBookingStatusChangeJobWithTimeout(firstBooking);

        BackgroundJobServiceMock.Reset();
        NewScope();

        SetCurrentUser(guest2);
        
        var res = await Mediator.Send(
            new CreateBookingCommand(
                GuestCount: 2,
                RestaurantId: restaurant.Id.Value,
                TableNumber: null,
                ScheduledAt: scheduledAt));
        res.IsFailed.Should().BeTrue();
        res.ShouldContain(TableErrors.NotFound);

        var bookingsCount = await NewDbContext().Bookings.CountAsync();
        bookingsCount.Should().Be(1);

        BackgroundJobServiceMock.Verify(
            s => s.Schedule(It.IsAny<Expression<Action<IBookingCancellationService>>>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }

    [Fact]
    public async Task When_TableNumberIsSpecified_And_TimeSlotsDoesNotOverlap_ShouldCreateBooking()
    {
        var users = await Users.CreateBase3Async();
        User guest = users[2], manager = users[1];
        var guest2 = await Users.CreateGuestAsync("bimba", "bimba@gmail.com", "+77777778899");
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        SetCurrentUser(guest);

        var scheduledAt1 = FakeTime.GetUtcNow().AddHours(1);
        var scheduledAt2 = scheduledAt1.AddHours(3); // Non-overlapping time slot

        NewScope();
        // Create a booking for the specified table
        var res1 = await Mediator.Send(
            new CreateBookingCommand(
                GuestCount: 4,
                RestaurantId: restaurant.Id.Value,
                TableNumber: 1,
                ScheduledAt: scheduledAt1));
        res1.IsSuccess.Should().BeTrue();
        
        var firstBooking = await NewDbContext().Bookings.AsNoTracking().FirstOrDefaultAsync();
        firstBooking.Should().NotBeNull();
        ShouldScheduleBookingStatusChangeJobWithTimeout(firstBooking);
        BackgroundJobServiceMock.Reset();
        
        var firstBookingId = firstBooking.Id;

        NewScope();
        SetCurrentUser(guest2);
        var res = await Mediator.Send(
            new CreateBookingCommand(
                GuestCount: 4,
                RestaurantId: restaurant.Id.Value,
                TableNumber: 1,
                ScheduledAt: scheduledAt2));
        res.IsSuccess.Should().BeTrue();
        
        var secondBooking = await NewDbContext().Bookings.SingleOrDefaultAsync(b => b.Id != firstBookingId);
        secondBooking.Should().NotBeNull();
        ShouldScheduleBookingStatusChangeJobWithTimeout(secondBooking);
        
        var bookingsCount = await NewDbContext().Bookings.CountAsync();
        bookingsCount.Should().Be(2);
    }
    
    [Fact]
    public async Task When_SeveralTablesAreAvailable_ShouldChooseTheOneWithSmallestCapacity()
    {
        var users = await Users.CreateBase3Async();
        User guest = users[2], manager = users[1];
        var restaurant = await Restaurants.CreateDefaultWithTables(manager.Id.Value,
            (1, 2), (2, 3), 
            (3, 5), (4, 5), (5, 10));
        SetCurrentUser(guest);

        var scheduledAt = FakeTime.GetUtcNow().AddHours(1);
        
        NewScope();
        var res = await Mediator.Send(
            new CreateBookingCommand(
                GuestCount: 5,
                RestaurantId: restaurant.Id.Value,
                TableNumber: null,
                ScheduledAt: scheduledAt));
        res.IsSuccess.Should().BeTrue();
        
        var booking = await NewDbContext().Bookings.SingleOrDefaultAsync();
        booking.Should().NotBeNull();
        booking.TableNumber.Should().BeOneOf(3, 4); // The table with the smallest capacity that fits 5 guests
        
        ShouldScheduleBookingStatusChangeJobWithTimeout(booking);
    }
}