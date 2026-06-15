namespace BookingSystem.Domain.Restaurants.ValueObjects;

public sealed record TableId(RestaurantId RestaurantId, int TableNumber);