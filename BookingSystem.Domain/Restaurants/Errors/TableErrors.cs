using BookingSystem.Domain.Common.Errors;
using FluentResults;

namespace BookingSystem.Domain.Restaurants.Errors;

public static class TableErrors
{
    public static readonly DomainError NotFound = new NotFoundError("Table.NotFound", "The table was not found");
    public static class Capacity
    {
        public static readonly DomainError OutOfRange = new ValidationError("Table.Capacity.OutOfRange", "Capacity must be greater than 0");
    }
}