using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.ValueObjects;
using FluentResults;

namespace BookingSystem.Application.Features.Bookings.Abstractions;

public interface IBookingCompletionService
{
    public Task<Result> Complete(Booking booking, CancellationToken cancellationToken = default);
    public Task<Result> Complete(BookingId bookingId, CancellationToken cancellationToken = default);
}