using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Restaurants;
using BookingSystem.Tests.Builders;

namespace BookingSystem.Tests.Services;

public class RestaurantTestDataService (AppDbContext dbContext)
{
    public async Task<Restaurant> CreateDefault(Guid ownerId)
    {
        var restaurant = new RestaurantBuilder().WithOwner(ownerId).Build();
        dbContext.Restaurants.Add(restaurant);
        await dbContext.SaveChangesAsync();
        return restaurant;
    }

    public async Task<Restaurant> CreateDefaultWithTables(Guid ownerId,
        params IEnumerable<(int tableNumber, int capacity)> tables)
    {
        var restaurant = new RestaurantBuilder().WithTables(tables).WithOwner(ownerId).Build();
        dbContext.Restaurants.Add(restaurant);
        await dbContext.SaveChangesAsync();
        return restaurant;
    }

    public async Task<Restaurant[]> CreateRestaurants(Action<RestaurantsBuilder> config)
    {
        var builder = new RestaurantsBuilder();
        config(builder);
        var restaurants = builder.Select(x => x.Build()).ToArray();
        dbContext.Restaurants.AddRange(restaurants);
        await dbContext.SaveChangesAsync();
        return restaurants;
    }
}