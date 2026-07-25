using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Restaurants.Errors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Features.FavoriteRestaurants.Commands.RemoveRestaurantFromFavorites;

public class RemoveRestaurantFromFavoritesHandler (AppDbContext dbContext, ICurrentUserService
     currentUserService): IRequestHandler<RemoveRestaurantFromFavoritesCommand, Result>
{
    public async Task<Result> Handle(RemoveRestaurantFromFavoritesCommand request, CancellationToken cancellationToken)
    {
        var rows = await dbContext.FavoriteRestaurants.Where(fr =>
                fr.RestaurantId == new RestaurantId(request.RestaurantId) &&
                fr.UserId == currentUserService.GetRequiredUserId())
            .ExecuteDeleteAsync(cancellationToken);
        if (rows == 0)
            return RestaurantErrors.NotFound;
        return Result.Ok();
    }
}