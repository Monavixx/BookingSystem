using BookingSystem.Application.Common.Options;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Restaurants;
using BookingSystem.Infrastructure.Services;
using BookingSystem.Tests.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BookingSystem.Tests.Infrastructure.Services;

public class UserBlockerTests(PostgresTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    private BookingOptions _bookingOptions = null!;
    private Base5Users _users = null!;

    private Restaurant _restaurant1 = null!,
        _restaurant2 = null!;

    protected override async ValueTask InitAsync()
    {
        _users = await Users.CreateBase5Async();
        var restaurants = await Restaurants.CreateRestaurants(builder => builder
            .AddRestaurant(_users.Manager, (1, 3), (2, 2), (3, 7))
            .AddRestaurant(_users.AnotherManager, (1, 3), (2, 8), (3, 11), (4, 8), (5, 2))
        );
        (_restaurant1, _restaurant2) = (restaurants[0], restaurants[1]);


        _bookingOptions = Scope.ServiceProvider.GetRequiredService<IOptions<BookingOptions>>().Value;
    }

    [Fact]
    public async Task When_UserHasEnoughViolations_ShouldBlockUser()
    {
        var bookings = await Bookings.CreateBookings(config =>
        {
            for (int i = 0; i < _bookingOptions.MaxBookingCancellation; i++)
                config.AddBooking(_users.Guest, _restaurant1, (i % 3) + 1, guestCount: 2,
                    status: BookingStatus.Canceled,
                    startTime: FakeTime.GetUtcNow().AddHours(i + 2));
        });
        DbContext.CancellationRecords.AddRange(bookings.Select(b =>
            CancellationRecord.Create(FakeTime, _users.Guest.Id, b.Id, CancellationReason.GuestRequest)));
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        NewScope();

        var userBlocker = Scope.ServiceProvider.GetRequiredService<UserBlocker>();
        var res = await userBlocker.BlockUserIfCancellationPolicyViolated(_users.Guest.Id);
        res.IsSuccess.Should().BeTrue();

        NewScope();
        var user = await DbContext.Users.FindAsync([_users.Guest.Id], TestContext.Current.CancellationToken);
        user.Should().NotBeNull();
        user.IsBlocked.Should().BeTrue();
        user.BlockedUntil.Should()
            .Be(FakeTime.GetUtcNow().Add(_bookingOptions.ViolationCancellationPolicyBlockDuration));
    }

    [Fact]
    public async Task When_UserDoesNotHaveEnoughViolations_ShouldNotBlockUser()
    {
        var bookings = await Bookings.CreateBookings(config =>
        {
            for (int i = 0; i < _bookingOptions.MaxBookingCancellation - 1; i++)
                config.AddBooking(_users.Guest, _restaurant1, (i % 3) + 1, guestCount: 2,
                    status: BookingStatus.Canceled,
                    startTime: FakeTime.GetUtcNow().AddHours(i + 2));
        });
        DbContext.CancellationRecords.AddRange(bookings.Select(b =>
            CancellationRecord.Create(FakeTime, _users.Guest.Id, b.Id, CancellationReason.GuestRequest)));
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        NewScope();

        var userBlocker = Scope.ServiceProvider.GetRequiredService<UserBlocker>();
        var res = await userBlocker.BlockUserIfCancellationPolicyViolated(_users.Guest.Id);
        res.IsSuccess.Should().BeTrue();

        NewScope();
        var user = await DbContext.Users.FindAsync([_users.Guest.Id], TestContext.Current.CancellationToken);
        user.Should().NotBeNull();
        user.IsBlocked.Should().BeFalse();
        user.BlockedUntil.Should().BeNull();
    }

    [Fact]
    public async Task When_AdminBeingChecked_ShouldNotBlock()
    {
        var bookings = await Bookings.CreateBookings(config =>
        {
            for (int i = 0; i < _bookingOptions.MaxBookingCancellation; i++)
                config.AddBooking(_users.Admin, _restaurant1, (i % 3) + 1, guestCount: 2,
                    status: BookingStatus.Canceled,
                    startTime: FakeTime.GetUtcNow().AddHours(i + 2));
        });
        DbContext.CancellationRecords.AddRange(bookings.Select(b =>
            CancellationRecord.Create(FakeTime, _users.Admin.Id, b.Id, CancellationReason.NoShow)));
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        NewScope();

        var userBlocker = Scope.ServiceProvider.GetRequiredService<UserBlocker>();
        var res = await userBlocker.BlockUserIfCancellationPolicyViolated(_users.Admin.Id);
        res.IsFailed.Should().BeTrue();

        NewScope();
        var user = await DbContext.Users.FindAsync([_users.Admin.Id], TestContext.Current.CancellationToken);
        user.Should().NotBeNull();
        user.IsBlocked.Should().BeFalse();
        user.BlockedUntil.Should().BeNull();
    }
    
    [Fact]
    public async Task When_UserDoesNotHaveEnoughViolationsBecauseSomeAreOld_ShouldNotBlockUser()
    {
        var bookings = await Bookings.CreateBookings(config =>
        {
            for (int i = 0; i < _bookingOptions.MaxBookingCancellation - 1; i++)
                config.AddBooking(_users.Guest, _restaurant1, (i % 3) + 1, guestCount: 2,
                    status: BookingStatus.Canceled,
                    startTime: FakeTime.GetUtcNow().AddHours(i + 2));
            // Add an old violation that is outside the cancellation period
            config.AddBooking(_users.Guest, _restaurant1, 1, guestCount: 2,
                status: BookingStatus.Canceled,
                startTime: FakeTime.GetUtcNow().AddDays((-_bookingOptions.BookingCancellationPeriod.Days) - 2));
        });
        DbContext.CancellationRecords.AddRange(bookings[..^1].Select(b =>
            CancellationRecord.Create(FakeTime, _users.Guest.Id, b.Id, CancellationReason.GuestRequest)));
        FakeTime.AdjustTime(FakeTime.GetUtcNow() - _bookingOptions.BookingCancellationPeriod.Add(TimeSpan.FromDays(1)));
        DbContext.CancellationRecords.Add(CancellationRecord.Create(FakeTime, _users.Guest.Id, bookings[^1].Id,
            CancellationReason.GuestRequest));
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        NewScope();

        var userBlocker = Scope.ServiceProvider.GetRequiredService<UserBlocker>();
        var res = await userBlocker.BlockUserIfCancellationPolicyViolated(_users.Guest.Id);
        res.IsSuccess.Should().BeTrue();

        NewScope();
        var user = await DbContext.Users.FindAsync([_users.Guest.Id], TestContext.Current.CancellationToken);
        user.Should().NotBeNull();
        user.IsBlocked.Should().BeFalse();
        user.BlockedUntil.Should().BeNull();
    }
}