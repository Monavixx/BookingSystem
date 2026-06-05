using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Common;

namespace BookingSystem.Domain.Bookings.Events;

public record BookingCreatedEvent(BookingId Id) : DomainEvent;