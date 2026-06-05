using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.Events;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Common;
using BookingSystem.Domain.Common.Errors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Domain.Users.ValueObjects;
using FluentResults;

namespace BookingSystem.Domain.Bookings;

public class Booking : AggregateRoot<BookingId>
{
    public UserId GuestId { get; private set; }
    public int GuestCount { get; private set; }
    public RestaurantId RestaurantId { get; private set; }
    public int TableNumber { get; private set; }
    public BookingTimeSlot TimeSlot { get; private set; } = null!;
    public BookingStatus Status { get; private set; } = BookingStatus.Pending;

    private Booking() { }

    public static Result<Booking> Create(UserId guestId, int guestCount, RestaurantId restaurantId, int tableNumber,
        BookingTimeSlot timeSlot)
    {
        if (guestCount <= 0)
            return Result.Fail<Booking>(BookingErrors.GuestCountOutOfRange);
        var booking = new Booking()
        {
            Id = BookingId.New(),
            GuestId = guestId,
            GuestCount = guestCount,
            RestaurantId = restaurantId,
            TableNumber = tableNumber,
            TimeSlot = timeSlot,
            Status = BookingStatus.Pending
        };
        booking.AddDomainEvent(new BookingCreatedEvent(booking.Id));
        return booking;
    }

    public Result ConfirmByGuest()
    {
        if (Status is not BookingStatus.Pending)
            return Result.Fail(new ConflictError("Booking.InvalidStatusTransition",
                $"Status {Status} cannot transition into {nameof(BookingStatus.ConfirmedByGuest)}"));
        Status = BookingStatus.ConfirmedByGuest;
        return Result.Ok();
    }

    public Result CancelBySystem()
    {
        if (Status is BookingStatus.Completed)
            return Result.Fail(BookingErrors.Status.InvalidStatusTransition);
        Status = BookingStatus.Canceled;
        return Result.Ok();
    }
}