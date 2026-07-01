using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Restaurants;
using Tests.Builders;

namespace Tests.Services;

public class RestaurantTestDataService (AppDbContext dbContext)
{
    public async Task<Restaurant> CreateDefault(Guid ownerId)
    {
        var restaurant = new RestaurantBuilder().WithOwner(ownerId).Build();
        dbContext.Restaurants.Add(restaurant);
        await dbContext.SaveChangesAsync();
        return restaurant;
    }
}