using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Restaurant.Errors;
using BookingSystem.Domain.Restaurant.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Restaurants.Commands.AddTableToRestaurant;

public class AddTableToRestaurantHandler (AppDbContext dbContext, ILogger<AddTableToRestaurantHandler> logger): IRequestHandler<AddTableToRestaurantCommand, Result>
{
    public async Task<Result> Handle(AddTableToRestaurantCommand request, CancellationToken cancellationToken)
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