using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Restaurant.Errors;

public static class RestaurantErrors
{
    public static class Description
    {
        public static readonly ValidationError TooLong = new ValidationError("Restaurant.Description.TooLong",
            $"Description must be at most {Restaurant.DescriptionMaxLength} characters long");
    }
}