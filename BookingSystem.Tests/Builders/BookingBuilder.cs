using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Restaurants;
using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Tests.Builders;

public class BookingBuilder
{
    private Guid GuestId { get; set; }
    private int GuestCount { get; set; } = 1;
    private Guid RestaurantId { get; set; }
    private int TableNumber { get; set; } = 1;
    private BookingTimeSlot TimeSlot { get; set; }
    private BookingStatus Status { get; set; } = BookingStatus.Pending;

    public BookingBuilder()
    {
        var now = DateTimeOffset.UtcNow;
        TimeSlot = BookingTimeSlot.Create(now.AddHours(1), now.AddHours(2)).Value;
    }
    
    public BookingBuilder WithGuest(Guid id) { GuestId = id; return this; }
    public BookingBuilder WithGuest(User guest) { GuestId = guest.Id.Value; return this; }
    public BookingBuilder WithGuestCount(int count) { GuestCount = count; return this; }
    public BookingBuilder WithRestaurant(Guid id) { RestaurantId = id; return this; }
    public BookingBuilder WithRestaurant(Restaurant restaurant) { RestaurantId = restaurant.Id.Value; return this; }
    public BookingBuilder WithTableNumber(int number) { TableNumber = number; return this; }
    public BookingBuilder WithStatus(BookingStatus status) { Status = status; return this; }
    public BookingBuilder WithTimeSlotNoChecking(DateTimeOffset start, TimeSpan duration) 
        { TimeSlot = BookingTimeSlot.__CreateWithNoChecking(start, start + duration); return this; }

    public Booking Build()
    {
        var booking = Booking.Create(new UserId(GuestId), GuestCount, new RestaurantId(RestaurantId), TableNumber, TimeSlot).Value;
        booking.__SetStatus(Status);
        return booking;
    }
}