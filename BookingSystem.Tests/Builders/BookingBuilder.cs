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
    private BookingTimeSlot? TimeSlot { get; set; } = null;
    private BookingStatus Status { get; set; } = BookingStatus.Pending;

    public BookingBuilder WithGuest(Guid id) { GuestId = id; return this; }
    public BookingBuilder WithGuest(User guest) { GuestId = guest.Id.Value; return this; }
    public BookingBuilder WithGuestCount(int count) { GuestCount = count; return this; }
    public BookingBuilder WithRestaurant(Guid id) { RestaurantId = id; return this; }
    public BookingBuilder WithRestaurant(Restaurant restaurant) { RestaurantId = restaurant.Id.Value; return this; }
    public BookingBuilder WithTableNumber(int number) { TableNumber = number; return this; }
    public BookingBuilder WithStatus(BookingStatus status) { Status = status; return this; }
    /// <summary>
    /// Sets booking time slot for the future booking.
    /// The default time slot starts 1 hour from now and lasts for 1.5 hours.
    /// </summary>
    public BookingBuilder WithTimeSlotNoChecking(DateTimeOffset start, TimeSpan duration) 
        { TimeSlot = BookingTimeSlot.__CreateWithNoChecking(start, start + duration); return this; }

    public Booking Build(TimeProvider? provider = null)
    {
        var booking = Booking.Create(new UserId(GuestId), GuestCount, new RestaurantId(RestaurantId), TableNumber, 
            TimeSlot ?? (provider is not null ? BookingTimeSlot.Create(provider.GetUtcNow().AddHours(1), provider.GetUtcNow().AddMinutes(90+60)).Value
            : throw new ArgumentException("You must either call WithTimeSlotNoChecking method or provide TimeProvider in arguments"))).Value;
        booking.__SetStatus(Status);
        return booking;
    }
}