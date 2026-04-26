using BookingSystem.Domain.Common;
using BookingSystem.Domain.Restaurant.ValueObjects;

namespace BookingSystem.Domain.Restaurant;

public class Table : Entity<TableId>
{
    private Table() { }

    public RestaurantId RestaurantId { get; private set; }
    public int TableNumber { get; private set; }
}