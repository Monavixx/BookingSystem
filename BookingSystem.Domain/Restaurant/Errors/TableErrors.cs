using BookingSystem.Domain.Common.Errors;
using FluentResults;

namespace BookingSystem.Domain.Restaurant.Errors;

public static class TableErrors
{
    public static class Capacity
    {
        public static readonly IError OutOfRange = new ValidationError("Table.Capacity.OutOfRange", "Capacity must be greater than 0");
    }
}