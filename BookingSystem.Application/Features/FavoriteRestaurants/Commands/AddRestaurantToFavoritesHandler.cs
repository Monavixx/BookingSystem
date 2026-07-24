using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.FavoriteRestaurants;
using BookingSystem.Domain.Restaurants.ValueObjects;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.FavoriteRestaurants.Commands;

public class AddRestaurantToFavoritesHandler (AppDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<AddRestaurantToFavoritesCommand, Result>
{
    public async Task<Result> Handle(AddRestaurantToFavoritesCommand request, CancellationToken cancellationToken)
    {
        var favoriteRestaurant = FavoriteRestaurant.Create(currentUserService.GetRequiredUserId(),
            new RestaurantId(request.RestaurantId));
        if (favoriteRestaurant.IsFailed) return favoriteRestaurant.ToResult();

        dbContext.FavoriteRestaurants.Add(favoriteRestaurant.Value);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}