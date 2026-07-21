namespace BookingSystem.Domain.Restaurants.ValueObjects;

public readonly record struct RestaurantId (Guid Value)
{
    public static RestaurantId Create() => new(Guid.CreateVersion7());
}