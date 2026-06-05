using BookingSystem.Domain.Common;
using MediatR;

namespace BookingSystem.Application.Common;

public interface IDomainEventHandler<in T> : INotificationHandler<T> where T : DomainEvent;