using BookingSystem.Application.Features.Restaurants.Commands.CreateRestaurant;
using FluentValidation;

namespace BookingSystem.Application.Features.Restaurants.Commands.UpdateRestaurant;

public class UpdateRestaurantValidator : AbstractValidator<UpdateRestaurantCommand>
{
    public UpdateRestaurantValidator()
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
        RuleFor(x => x.Contact)
            .ChildRules(v =>
            {
                v.RuleFor(x => x.Email)
                    .EmailAddress();
                v.RuleFor(x => x.PhoneNumber)
                    .NotEmpty();
            });
    }
}