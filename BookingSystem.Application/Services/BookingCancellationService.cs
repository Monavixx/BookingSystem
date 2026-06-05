using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings.ValueObjects;

namespace BookingSystem.Application.Services;

public class BookingCancellationService(AppDbContext dbContext) : IBookingCancellationService
{
    public async Task CancelIfPendingAsync(BookingId bookingId)
    {
        var booking = await dbContext.Bookings.FindAsync(bookingId);
        if (booking is null or {Status: not BookingStatus.Pending}) return;
        
        booking.CancelBySystem();
        await dbContext.SaveChangesAsync();
    }

    public Task CancelIfNotConfirmedAsync(BookingId bookingId)
    {
        var booking = dbContext.Bookings.Find(bookingId);
        if (booking is null or { Status: not (BookingStatus.Pending or BookingStatus.ConfirmedByGuest) })
            return Task.CompletedTask;
        
        booking.CancelBySystem();
        return dbContext.SaveChangesAsync();
    }
}