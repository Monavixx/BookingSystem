using BookingSystem.Domain.Bookings.ValueObjects;

namespace BookingSystem.Application.Features.Bookings.Queries.Get;

public record GetBookingResponse(
    Guid GuestId,
    int GuestCount,
    Guid RestaurantId,
    int TableNumber,
    BookingStatus Status,
    DateTimeOffset Start,
    DateTimeOffset End)
{
}