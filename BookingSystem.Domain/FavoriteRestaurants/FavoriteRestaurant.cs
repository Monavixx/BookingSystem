using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Domain.FavoriteRestaurants;

public sealed class FavoriteRestaurant
{
    public UserId UserId { get; private set; }
    public RestaurantId RestaurantId { get; private set; }
}