using FluentValidation;

namespace BookingSystem.Application.Features.Restaurants.Commands.AddTable;

public class AddTableValidator : AbstractValidator<AddTableCommand>
{
    public AddTableValidator()
    {
        RuleFor(x => x.Capacity)
            .GreaterThan(0);
    }
}