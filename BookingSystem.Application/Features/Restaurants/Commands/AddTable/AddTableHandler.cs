using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Restaurants.Errors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Restaurants.Commands.AddTable;

public class AddTableHandler (AppDbContext dbContext, ILogger<AddTableHandler> logger): IRequestHandler<AddTableCommand, Result>
{
    public async Task<Result> Handle(AddTableCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await dbContext.Restaurants.FindAsync([new RestaurantId(request.RestaurantId)], cancellationToken);
        if (restaurant is null) return Result.Fail(RestaurantErrors.NotFound);
        if (restaurant.AddTable(request.TableNumber, request.Capacity) is { IsFailed: true } addTableResult)
            return addTableResult;
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Added table {TableNumber} with capacity {TableCapacity} to restaurant {RestaurantId}",
            request.TableNumber, request.Capacity, request.RestaurantId);
        return Result.Ok();
    }
}