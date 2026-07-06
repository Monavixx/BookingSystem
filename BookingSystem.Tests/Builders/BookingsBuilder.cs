using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Restaurants;
using BookingSystem.Domain.Users;

namespace BookingSystem.Tests.Builders;

public class BookingsBuilder : List<BookingBuilder>
{
    public BookingBuilder New()
    {
        var builder = new BookingBuilder();
        Add(builder);
        return builder;
    }

    public BookingsBuilder AddBooking(User guest, Restaurant restaurant, int tableNumber,
        int guestCount = 2, BookingStatus status = BookingStatus.Pending, DateTimeOffset? startTime = null,
        TimeSpan? duration = null)
    {
        var builder = new BookingBuilder()
            .WithGuest(guest)
            .WithGuestCount(guestCount)
            .WithRestaurant(restaurant)
            .WithTableNumber(tableNumber)
            .WithStatus(status)
            .WithTimeSlotNoChecking(startTime ?? DateTimeOffset.UtcNow.AddHours(1),
                duration ?? TimeSpan.FromMinutes(90));
        Add(builder);
        return this;
    }
}