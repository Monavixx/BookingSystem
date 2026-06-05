using MediatR;

namespace BookingSystem.Domain.Common;

public abstract record DomainEvent : INotification
{
    public DateTime OccuredOn { get; } = DateTime.UtcNow;
}