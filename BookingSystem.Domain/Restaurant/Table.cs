using BookingSystem.Domain.Common;
using BookingSystem.Domain.Restaurant.Errors;
using BookingSystem.Domain.Restaurant.ValueObjects;
using FluentResults;

namespace BookingSystem.Domain.Restaurant;

public class Table : IEntity
{
    private Table() { }

    public RestaurantId RestaurantId { get; private set; }
    public int TableNumber { get; private set; }
    public int Capacity { get; private set; }

    public static Result<Table> Create(RestaurantId restaurantId, int tableNumber, int capacity)
    {
        if (capacity <= 0) return Result.Fail<Table>(TableErrors.Capacity.OutOfRange);
        return new Table()
        {
            RestaurantId = restaurantId,
            TableNumber = tableNumber,
            Capacity = capacity
        };
    }

    public uint RowVersion { get; set; }
}