using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Domain.Users.ValueObjects;
using FluentResults;

namespace BookingSystem.Domain.FavoriteRestaurants;

public sealed class FavoriteRestaurant
{
    private FavoriteRestaurant(){}
    public UserId UserId { get; private set; }
    public RestaurantId RestaurantId { get; private set; }

    public static Result<FavoriteRestaurant> Create(UserId userId, RestaurantId restaurantId)
        => new FavoriteRestaurant()
        {
            UserId = userId,
            RestaurantId = restaurantId
        };
}