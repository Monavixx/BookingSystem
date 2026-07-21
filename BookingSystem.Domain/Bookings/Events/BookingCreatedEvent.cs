using BookingSystem.Domain.Common;

namespace BookingSystem.Domain.Bookings.Events;

public record BookingCreatedEvent(Booking Booking) : DomainEvent;