using System.Data;
using BookingSystem.Application.Features.Bookings.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings.ValueObjects.Helpers;
using BookingSystem.Domain.Restaurants.ValueObjects;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

public class TableAvailabilityChecker(AppDbContext dbContext) : ITableAvailabilityChecker
{
    private const string Sql =
        """
            SELECT 1 FROM Bookings
            WHERE restaurant_id = @RestaurantId
              AND table_number = @TableNumber
              AND start_time < @To
              AND end_time > @From
              AND status != ALL(@FinalStatuses)
            LIMIT 1
        """;

    public async Task<bool> IsTableAvailableAsync(TableId tableId, DateTimeOffset from, DateTimeOffset to,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();
        return (await connection.QueryFirstOrDefaultAsync<int?>(Sql,
            new
            {
                RestaurantId = tableId.RestaurantId.Value, tableId.TableNumber, From = from.UtcDateTime,
                To = to.UtcDateTime,
                FinalStatuses = BookingStatusHelper.FinalIntStatuses
            }, dbTransaction)) is null;
    }
}