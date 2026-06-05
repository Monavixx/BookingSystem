using BookingSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookingSystem.Application.Persistence.Interceptors;

public class DomainEventInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        await PublishDomainEvents(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async ValueTask PublishDomainEvents(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is null) return;

        var events = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .SelectMany(x => x.Entity.DomainEvents);
        
        foreach (var @event in events)
        {
            await publisher.Publish(@event, cancellationToken);
        }
    }
}