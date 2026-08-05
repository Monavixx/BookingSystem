using BookingSystem.Domain.Common;

namespace BookingSystem.Domain.Users.DomainEvents;

public record UserUpdatedEvent(User User) : DomainEvent;
