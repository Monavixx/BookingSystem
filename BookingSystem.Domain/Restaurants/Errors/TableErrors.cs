using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Restaurants.Errors;

public static class TableErrors
{
    public static readonly DomainError NotFound = new NotFoundError("Table.NotFound", "The table was not found");

    public static readonly DomainError AccessDenied =
        new ForbiddenError("Table.AccessDenied", "Only the owner of the restaurant can modify this table");
    public static class Capacity
    {
        public static readonly DomainError OutOfRange = new ValidationError("Table.Capacity.OutOfRange", "Capacity must be greater than 0");
    }
}