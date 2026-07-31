using BookingSystem.Domain.Common.ValueObjects;
using BookingSystem.Domain.Restaurants;
using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Tests.Builders;

public class RestaurantBuilder
{
    private Address _address =
        Address.Create("Russia", "Moskovskaya Oblast", "Moscow", "Georgia St.",
            "10Б", "452", null).Value;
    private string _contactPhoneNumber = "+79009006666";
    private string _email = "rest_owner@gmail.com";
    private string? _description;
    private string? _imageUrl;

    private WorkingSchedule? _workingSchedule =
        WorkingSchedule.Create(
            Enumerable.Range(0, 7)
                .Select(i => DayOfWeekSchedule.Create(
                    (DayOfWeek)i, new TimeOnly(9, 0), new TimeOnly(21, 0)
                ))).Value;
    private readonly List<(int, int)> _tables = [(1, 4)];
    private Guid _ownerId = Guid.Empty;

    public RestaurantBuilder WithAddress(Address address) { _address = address; return this; }
    public RestaurantBuilder WithContactPhoneNumber(string contactPhoneNumber) { _contactPhoneNumber = contactPhoneNumber; return this; }
    public RestaurantBuilder WithEmail(string email) { _email = email; return this; }
    public RestaurantBuilder WithDescription(string? description) { _description = description; return this; }
    public RestaurantBuilder WithImageUrl(string? imageUrl) { _imageUrl = imageUrl; return this; }
    public RestaurantBuilder WithWorkingSchedule(WorkingSchedule? workingSchedule) { _workingSchedule = workingSchedule; return this; }
    public RestaurantBuilder WithOwner(Guid ownerId) { _ownerId = ownerId; return this; }
    public RestaurantBuilder AddTable(int tableNumber, int capacity) { _tables.Add((tableNumber, capacity)); return this; }
    public RestaurantBuilder WithTables(params IEnumerable<(int tableNumber, int capacity)> tables) { _tables.Clear(); _tables.AddRange(tables); return this; }

    public Restaurant Build()
    {
        var restaurant = Restaurant.Create(
            _address.Country, _address.State, _address.City, _address.Street, _address.HouseNumber,
            _address.ApartmentNumber, _address.ZipCode, _contactPhoneNumber, _email, _description, _imageUrl,
            new UserId(_ownerId)).Value;
        foreach (var table in _tables)
            restaurant.AddTable(table.Item1, table.Item2);
        return restaurant;
    }
}
