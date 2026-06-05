using BookingSystem.Domain.Bookings.ValueObjects;

namespace BookingSystem.Application.Common.Abstractions;

public interface IBookingCancellationService
{
    public Task CancelIfPendingAsync(BookingId bookingId);
    public Task CancelIfNotConfirmedAsync(BookingId bookingId);
}