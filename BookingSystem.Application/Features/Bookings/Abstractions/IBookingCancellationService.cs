using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.ValueObjects;
using FluentResults;

namespace BookingSystem.Application.Features.Bookings.Abstractions;

public interface IBookingCancellationService
{
    Task<Result<bool>> CancelAsync(BookingId bookingId, CancellationReason reason);
    Task<Result<bool>> CancelAsync(Booking booking, CancellationReason reason);
}