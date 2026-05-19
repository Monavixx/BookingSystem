using FluentValidation;

namespace BookingSystem.Application.Features.Auth.Commands.Refresh;

public class RefreshValidator : AbstractValidator<RefreshCommand>
{
    public RefreshValidator()
    {
        RuleFor(c => c.RefreshToken)
            .NotEmpty();
    }
}