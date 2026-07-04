using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings;
using BookingSystem.Tests.Builders;

namespace BookingSystem.Tests.Services;

public class BookingTestDataService (AppDbContext dbContext)
{
    public async Task<Booking> Create(Action<BookingBuilder> config)
    {
        var builder = new BookingBuilder();
        config(builder);
        var booking = builder.Build();
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();
        return booking;
    }
}