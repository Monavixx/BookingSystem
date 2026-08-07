using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Restaurants;
using BookingSystem.Domain.Users;
using BookingSystem.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Tests.Infrastructure.Services;

public class BookingCancellationServiceTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    private User _manager = null!,
        _anotherManager = null!,
        _guest = null!;
    private Restaurant _restaurant = null!;
    private BookingCancellationService _bookingCancellationService = null!;
    protected override async ValueTask InitAsync()
    {
        _manager = await Users.CreateManagerAsync();
        _anotherManager = await Users.CreateAnotherManagerAsync();
        _guest = await Users.CreateGuestAsync();
        _restaurant = await Restaurants.CreateDefault(_manager.Id.Value);
        _bookingCancellationService = Scope.ServiceProvider.GetRequiredService<BookingCancellationService>();
    }

    [Fact]
    public async Task CancelAsync_WhenBookingExists_And_IsNotFinished_ShouldCancelBooking()
    {
        var booking = await Bookings.Create(b => b
                .WithStatus(BookingStatus.Confirmed)
                .WithRestaurant(_restaurant)
                .WithGuest(_guest)
                .WithTableNumber(1));
        SetReadOnlyCurrentUser(_manager);
        var res = await _bookingCancellationService.CancelAsync(booking.Id, CancellationReason.ManagerOrAdminBeenAskedByGuest);

        res.ShouldBeSuccess();

        NewScope();
        var dbBooking = await DbContext.Bookings.FindAsync([booking.Id], TestContext.Current.CancellationToken);
        dbBooking.Should().NotBeNull();
        dbBooking.Status.Should().Be(BookingStatus.Canceled);
    }

    [Fact]
    public async Task CancelAsync_WhenBookingIsFinished_ShouldReturnError()
    {
        var booking = await Bookings.Create(b => b
                .WithStatus(BookingStatus.Completed)
                .WithRestaurant(_restaurant)
                .WithGuest(_guest)
                .WithTableNumber(1));
        SetReadOnlyCurrentUser(_guest);
        var res = await _bookingCancellationService.CancelAsync(booking.Id, CancellationReason.GuestRequest);

        res.ShouldContain(BookingErrors.Status.InvalidStatusOrReasonToCancelCode);
    }

    [Fact]
    public async Task CancelAsync_WhenBookingDoesNotExist_ShouldReturnError()
    {
        SetReadOnlyCurrentUser(_guest);
        var res = await _bookingCancellationService.CancelAsync(BookingId.New(), CancellationReason.GuestRequest);

        res.ShouldContain(BookingErrors.NotFound);
    }
}
