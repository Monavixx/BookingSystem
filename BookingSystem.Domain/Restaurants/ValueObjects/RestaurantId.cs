namespace BookingSystem.Domain.Restaurants.ValueObjects;

public readonly record struct RestaurantId (Guid Value)
{
    public static RestaurantId New() => new(Guid.CreateVersion7());
}