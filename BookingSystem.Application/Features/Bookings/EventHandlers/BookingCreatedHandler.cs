using BookingSystem.Application.Common;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.Options;
using BookingSystem.Domain.Bookings.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookingSystem.Application.Features.Bookings.EventHandlers;

public class BookingCreatedHandler(IBackgroundJobService backgroundJobService, ILogger<BookingCreatedHandler> logger, IOptions<BookingOptions> options)
    : IDomainEventHandler<BookingCreatedEvent>
{
    public Task Handle(BookingCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Handling {nameof(BookingCreatedEvent)}");
        backgroundJobService.Schedule<IBookingCancellationService>(
            s => s.CancelIfPendingAsync(notification.Booking.Id),
            TimeSpan.FromMinutes(options.Value.GuestConfirmationTimeoutMinutes));
        backgroundJobService.Schedule<IBookingCancellationService>(
            s => s.CancelIfNotConfirmedAsync(notification.Booking.Id),
            notification.Booking.TimeSlot.Start);
        return Task.CompletedTask;
    }
}