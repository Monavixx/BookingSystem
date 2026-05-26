using FluentValidation;

namespace BookingSystem.Application.Features.Restaurants.Commands.AddTableToRestaurant;

public class AddTableToRestaurantValidator : AbstractValidator<AddTableToRestaurantCommand>
{
    public AddTableToRestaurantValidator()
    {
        RuleFor(x => x.Capacity)
            .GreaterThan(0);
    }
}