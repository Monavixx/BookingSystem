using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Restaurants.Errors;

public static class RestaurantErrors
{
    public static readonly DomainError NotFound = new NotFoundError("Restaurant.NotFound", "Restaurant not found");

    public static readonly DomainError AccessError =
        new ForbiddenError("Restaurant.AccessDenied", "Access to this restaurant is denied");
    public static class Description
    {
        public static readonly ValidationError TooLong = new ValidationError("Restaurant.Description.TooLong",
            $"Description must be at most {Restaurant.DescriptionMaxLength} characters long");
    }
}