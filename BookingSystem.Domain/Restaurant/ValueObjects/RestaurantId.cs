namespace BookingSystem.Domain.Restaurant.ValueObjects;

public readonly record struct RestaurantId (Guid Value)
{
    public static RestaurantId Create() => new RestaurantId { Value = Guid.CreateVersion7() };
}