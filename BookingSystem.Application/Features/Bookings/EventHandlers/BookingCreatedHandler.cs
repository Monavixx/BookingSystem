using BookingSystem.Application.Common;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Domain.Bookings.Events;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Bookings.EventHandlers;

public class BookingCreatedHandler(IBackgroundJobService backgroundJobService, ILogger<BookingCreatedHandler> logger) : IDomainEventHandler<BookingCreatedEvent>
{
    public Task Handle(BookingCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Handling {nameof(BookingCreatedEvent)}");
        backgroundJobService.Schedule<IBookingCancellationService>(s => s.CancelIfPendingAsync(notification.Id),
            TimeSpan.FromMinutes(5));
        return Task.CompletedTask;
    }
}