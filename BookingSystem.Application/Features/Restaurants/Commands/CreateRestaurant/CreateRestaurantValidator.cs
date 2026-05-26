using FluentValidation;

namespace BookingSystem.Application.Features.Restaurants.Commands.CreateRestaurant;

public class CreateRestaurantValidator : AbstractValidator<CreateRestaurantCommand>
{
    public CreateRestaurantValidator()
    {
        RuleFor(x => x.Address)
            .NotNull()
            .ChildRules(v =>
                {
                    v.RuleFor(x => x.Country)
                        .NotEmpty();
                    v.RuleFor(x => x.State)
                        .Must(x => x?.Length is not 0)
                        .WithMessage("State can be either null or not empty");
                    v.RuleFor(x => x.City)
                        .Must(x => x?.Length is not 0)
                        .WithMessage("City can be either null or not empty");
                    v.RuleFor(x => x.Street)
                        .Must(x => x?.Length is not 0)
                        .WithMessage("Street can be either null or not empty");
                    v.RuleFor(x => x.HouseNumber)
                        .Must(x => x?.Length is not 0)
                        .WithMessage("HouseNumber can be either null or not empty");
                    v.RuleFor(x => x.ApartmentNumber)
                        .Must(x => x?.Length is not 0)
                        .WithMessage("ApartmentNumber can be either null or not empty");
                    v.RuleFor(x => x.ZipCode)
                        .Must(x => x?.Length is not 0)
                        .WithMessage("ZipCode can be either null or not empty");
                }
            );
    }
}