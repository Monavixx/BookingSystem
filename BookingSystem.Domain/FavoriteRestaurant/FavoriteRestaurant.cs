using BookingSystem.Domain.Restaurant.ValueObjects;
using BookingSystem.Domain.User.ValueObjects;

namespace BookingSystem.Domain.FavoriteRestaurant;

public sealed class FavoriteRestaurant
{
    public UserId UserId { get; private set; }
    public RestaurantId RestaurantId { get; private set; }
}