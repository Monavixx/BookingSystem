using BookingSystem.Domain.Common;

namespace BookingSystem.Domain.Bookings.Events;

public record BookingGuestSeatedEvent(Booking Booking) : DomainEvent;