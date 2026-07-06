using BookingSystem.Domain.Users;

namespace BookingSystem.Tests.Builders;

public class RestaurantsBuilder : List<RestaurantBuilder>
{
    public RestaurantBuilder New()
    {
        var builder = new RestaurantBuilder();
        Add(builder);
        return builder;
    }
    public RestaurantsBuilder AddRestaurant(User manager, params IEnumerable<(int tableNumber, int capacity)> tables)
    {
        var builder = new RestaurantBuilder().WithOwner(manager.Id.Value).WithTables(tables);
        Add(builder);
        return this;
    }
}