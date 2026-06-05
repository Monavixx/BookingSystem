using BookingSystem.Domain.Common;
using BookingSystem.Domain.Restaurants;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Domain.Users;

public sealed class Manager : IEntity
{
    private Manager(){}

    private readonly List<Restaurant> _restaurants = [];
    public IReadOnlyCollection<Restaurant> Restaurants => _restaurants;

    public UserId UserId { get; private set; }
    public User User { get; private set; } = null!;
    
    public uint RowVersion { get; set; }

    public static Manager Create(UserId userId)
    {
        return new Manager { UserId = userId };
    }

    public void AddRestaurant(Restaurant restaurant)
    {
        _restaurants.Add(restaurant);
    }
}