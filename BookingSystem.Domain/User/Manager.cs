using BookingSystem.Domain.Common;
using BookingSystem.Domain.User.ValueObjects;

namespace BookingSystem.Domain.User;

public sealed class Manager : IEntity
{
    private Manager(){}

    private readonly List<Restaurant.Restaurant> _restaurants = [];
    public IReadOnlyCollection<Restaurant.Restaurant> Restaurants => _restaurants;

    public UserId UserId { get; private set; }
    public User User { get; private set; } = null!;
    
    public uint RowVersion { get; private set; }

    public static Manager Create(UserId userId)
    {
        return new Manager { UserId = userId };
    }

    public void AddRestaurant(Restaurant.Restaurant restaurant)
    {
        _restaurants.Add(restaurant);
    }
}