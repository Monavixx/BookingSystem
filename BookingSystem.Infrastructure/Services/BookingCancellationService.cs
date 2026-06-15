using BookingSystem.Application.Features.Bookings.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings.ValueObjects;

namespace BookingSystem.Infrastructure.Services;

public class BookingCancellationService(AppDbContext dbContext) : IBookingCancellationService
{
    public async Task CancelIfPendingAsync(BookingId bookingId)
    {
        var booking = await dbContext.Bookings.FindAsync(bookingId);
        if (booking is null or { Status: not BookingStatus.Pending }) return;

        booking.CancelBySystem();
        await dbContext.SaveChangesAsync();
    }

    public async Task CancelIfNotConfirmedAsync(BookingId bookingId)
    {
        var booking = await dbContext.Bookings.FindAsync(bookingId);
        if (booking is null or
            { Status: BookingStatus.Canceled or not (BookingStatus.Pending or BookingStatus.ConfirmedByGuest) })
            return;

        booking.CancelBySystem();
        await dbContext.SaveChangesAsync();
    }
}