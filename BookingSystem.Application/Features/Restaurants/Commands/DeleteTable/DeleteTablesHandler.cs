using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Restaurants.Errors;
using BookingSystem.Domain.Users;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Features.Restaurants.Commands.DeleteTable;

public class DeleteTablesHandler(AppDbContext dbContext, ICurrentUserService currentUserService)
    : IRequestHandler<DeleteTablesCommand, Result>
{
    public async Task<Result> Handle(DeleteTablesCommand request, CancellationToken cancellationToken)
    {
        var restaurantIds = request.Commands.Select(c => c.RestaurantId.Value).ToList();
        var tableNumbers = request.Commands.Select(c => c.TableNumber).ToList();
        var tablesInfo = await dbContext.Tables.FromSqlInterpolated(
                $"""
                SELECT t.*, t.xmin FROM tables t
                INNER JOIN unnest({restaurantIds}, {tableNumbers}) AS c(restaurant_id, table_number)
                ON t.restaurant_id = c.restaurant_id AND t.table_number = c.table_number
                """)
            .Select(t => new { Table = t, RestaurantOwnerId = t.Restaurant.OwnerId })
            .ToListAsync(cancellationToken);
        if (tablesInfo.Count < tableNumbers.Count)
            return TableErrors.NotFound.CloneWithMessage(
                "One or more of the provided tables do not exist.");

        var curUser = await currentUserService.GetUserAsync();
        if (tablesInfo.Any(t => t.RestaurantOwnerId != currentUserService.UserId &&
                                curUser?.Role is not UserRole.Admin))
            return TableErrors.AccessDenied.CloneWithMessage(
                "You are not allowed to delete one or more of the provided tables.");

        dbContext.Tables.RemoveRange(tablesInfo.Select(t => t.Table));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
