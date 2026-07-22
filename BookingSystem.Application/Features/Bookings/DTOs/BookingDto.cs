using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.ValueObjects;
using JetBrains.Annotations;

namespace BookingSystem.Application.Features.Bookings.DTOs;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record BookingDto(
    Guid Id,
    Guid GuestId,
    int GuestCount,
    Guid RestaurantId,
    int TableNumber,
    BookingStatus Status,
    DateTimeOffset Start,
    DateTimeOffset End)
{
    public BookingDto(Booking booking) : this(
        booking.Id.Value,
        booking.GuestId.Value,
        booking.GuestCount,
        booking.RestaurantId.Value,
        booking.TableNumber,
        booking.Status,
        booking.TimeSlot.Start,
        booking.TimeSlot.End)
    { }
}