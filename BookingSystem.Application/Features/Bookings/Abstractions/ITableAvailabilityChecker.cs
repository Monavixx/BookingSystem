using System.Data;
using BookingSystem.Domain.Restaurants.ValueObjects;

namespace BookingSystem.Application.Features.Bookings.Abstractions;

public interface ITableAvailabilityChecker
{
    /// <param name="from">including</param>
    /// <param name="to">excluding</param>
    Task<bool> IsTableAvailableAsync(TableId tableId, DateTimeOffset from, DateTimeOffset to,
        IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default);
}