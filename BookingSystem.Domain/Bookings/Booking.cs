using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.Events;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Bookings.ValueObjects.Helpers;
using BookingSystem.Domain.Common;
using BookingSystem.Domain.Common.Errors;
using BookingSystem.Domain.Restaurants;
using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.ValueObjects;
using FluentResults;

namespace BookingSystem.Domain.Bookings;

public class Booking : AggregateRoot<BookingId>
{
    public UserId GuestId { get; private set; }
    public User Guest { get; private set; } = null!;
    public int GuestCount { get; private set; }
    public RestaurantId RestaurantId { get; private set; }
    public int TableNumber { get; private set; }
    public Table Table { get; private set; } = null!;
    public BookingTimeSlot TimeSlot { get; private set; } = null!;
    public BookingStatus Status { get; private set; } = BookingStatus.Pending;
    public TableId TableId => new(RestaurantId, TableNumber);
    public CancellationRecord? CancellationRecord { get; private set; }

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
        booking.AddDomainEvent(new BookingCreatedEvent(booking));
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

    public Result Confirm()
    {
        if(Status is not BookingStatus.ConfirmedByGuest)
            return Result.Fail(new ConflictError("Booking.InvalidStatusTransition",
                $"Status {Status} cannot transition into {nameof(BookingStatus.Confirmed)}"));
        Status = BookingStatus.Confirmed;
        return Result.Ok();
    }

    /// <remarks>Can affect only Status</remarks>
    public Result GuestSeated()
    {
        if (CanGuestSit() is { IsFailed: true } failed)
            return failed;
        Status = BookingStatus.Seated;
        AddDomainEvent(new BookingGuestSeatedEvent(this));
        return Result.Ok();
    }
    public Result CanGuestSit()
    {
        return Result.OkIf(Status is BookingStatus.Confirmed, new ConflictError("Booking.InvalidStatusTransition",
            $"Status {Status} cannot transition into {nameof(BookingStatus.Seated)}"));
    }

    public Result Complete()
    {
        if (Status is not BookingStatus.Seated)
            return Result.Fail(new ConflictError("Booking.InvalidStatusTransition",
                $"Status {Status} cannot transition into {nameof(BookingStatus.Completed)}"));
        Status = BookingStatus.Completed;
        return Result.Ok();
    }

    /// <returns>true if it's attributable to the guest; otherwise, false</returns>
    public Result<bool> Cancel(CancellationReason reason)
    {
        if (!CanBeCanceled(reason))
            return BookingErrors.Status.InvalidStatusOrReasonToCancel(Status, reason);
        var isAttributableToGuest = IsAttributableToGuest(reason);
        Status = BookingStatus.Canceled;
        return Result.Ok(isAttributableToGuest);
    }

    public bool CanBeCanceled(CancellationReason reason) =>
        !IsFinished() && Status is not BookingStatus.Seated &&
        (reason switch
        {
            // CancellationReason.GuestRequest => true,
            // CancellationReason.ManagerOrAdminRequest => true,
            // CancellationReason.ManagerOrAdminBeenAskedByGuest => true,
            CancellationReason.NoShow => Status is BookingStatus.Confirmed,
            CancellationReason.PendingTimeout => Status is BookingStatus.Pending,
            CancellationReason.ManagerHasNotConfirmed => Status is BookingStatus.ConfirmedByGuest,
            _ => true
        });

    public bool IsAttributableToGuest(CancellationReason reason)
        => reason is (CancellationReason.GuestRequest or CancellationReason.ManagerOrAdminBeenAskedByGuest
            or CancellationReason.NoShow);

    public BookingAvailabilityState GetAvailabilityState(DateTimeOffset now) =>
        now switch
        {
            { } when now < TimeSlot.Start => BookingAvailabilityState.Early,
            { } when now > TimeSlot.End => BookingAvailabilityState.Expired,
            _ => BookingAvailabilityState.Valid
        };

    public bool IsFinished() => Status.IsFinal();
    
    
    // Methods that can only be used in tests
    
    internal void __SetStatus(BookingStatus status) => Status = status;
}