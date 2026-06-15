using BookingSystem.Application.Common;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Features.Bookings.Commands.Complete.CompleteBySystem;
using BookingSystem.Domain.Bookings.Events;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.EventHandlers;

public class BookingGuestSeatedEventHandler(IBackgroundJobService backgroundJobService)
    : IDomainEventHandler<BookingGuestSeatedEvent>
{
    public Task Handle(BookingGuestSeatedEvent notification, CancellationToken cancellationToken)
    {
        backgroundJobService.Schedule<IMediator>(
            // ReSharper disable once MethodSupportsCancellation
            s => s.Send(new CompleteBookingBySystemCommand(notification.Booking.Id.Value)),
            notification.Booking.TimeSlot.End);
        return Task.CompletedTask;
    }
}