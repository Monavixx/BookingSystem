using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Common.Errors;
using BookingSystem.Domain.Restaurant.Errors;
using BookingSystem.Domain.Restaurant.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Restaurants.Commands.DeleteRestaurant;

public class DeleteRestaurantHandler(AppDbContext dbContext, ILogger<DeleteRestaurantHandler> logger)
    : IRequestHandler<DeleteRestaurantCommand, Result>
{
    public async Task<Result> Handle(DeleteRestaurantCommand request, CancellationToken cancellationToken)
    {
        int rows = await dbContext.Restaurants
            .Where(r => r.Id == new RestaurantId(request.RestaurantId))
            .ExecuteDeleteAsync(cancellationToken);
        if (rows <= 0) return Result.Fail(RestaurantErrors.NotFound);
        if (rows > 1)
            return Result.Fail(new InternalServerError("Restaurants.Ambiguous",
                "There are two restaurants with the same id"));
        logger.LogInformation("Restaurant {RestaurantId} deleted", request.RestaurantId);
        return Result.Ok();
    }
}