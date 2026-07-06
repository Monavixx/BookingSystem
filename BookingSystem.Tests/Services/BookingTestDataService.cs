using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings;
using BookingSystem.Tests.Builders;

namespace BookingSystem.Tests.Services;

public class BookingTestDataService (AppDbContext dbContext, TimeProvider timeProvider)
{
    public async Task<Booking> Create(Action<BookingBuilder> config)
    {
        var builder = new BookingBuilder();
        config(builder);
        var booking = builder.Build(timeProvider);
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();
        return booking;
    }
    
    public async Task<Booking[]> CreateBookings(Action<BookingsBuilder> config)
    {
        var builder = new BookingsBuilder();
        config(builder);
        var bookings = builder.Select(x=>x.Build(timeProvider)).ToArray();
        dbContext.Bookings.AddRange(bookings);
        await dbContext.SaveChangesAsync();
        return bookings;
    }
}