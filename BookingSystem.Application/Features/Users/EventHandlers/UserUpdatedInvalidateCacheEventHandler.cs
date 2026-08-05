using BookingSystem.Application.Common;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Domain.Users.DomainEvents;

namespace BookingSystem.Application.Features.Users.EventHandlers;

public class UserUpdatedInvalidateCacheEventHandler(IUserCache userCache) : IDomainEventHandler<UserUpdatedEvent>
{
    public async Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await userCache.Invalidate(notification.User.Id);
    }
}
