using System.Data;
using BookingSystem.Application.Features.Bookings.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Restaurants.ValueObjects;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

public class TableAvailabilityChecker(AppDbContext dbContext) : ITableAvailabilityChecker
{
    private static readonly string Sql =
        $"""
        SELECT EXISTS(
            SELECT 1 FROM Bookings
            WHERE restaurant_id = @RestaurantId
              AND table_number = @TableNumber
              AND start_time < @To
              AND end_time > @From
              AND status NOT IN ({(int)BookingStatus.Canceled},{(int)BookingStatus.NoShow})
        )
        """;

    public async Task<bool> IsTableAvailableAsync(TableId tableId, DateTimeOffset from, DateTimeOffset to,
        IDbTransaction? dbTransaction = null,
        CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();
        return !await connection.ExecuteScalarAsync<bool>(Sql,
            new { RestaurantId = tableId.RestaurantId.Value, tableId.TableNumber, From = from.DateTime, To = to.DateTime }, dbTransaction);
    }
}