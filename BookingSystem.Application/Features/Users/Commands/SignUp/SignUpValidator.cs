using BookingSystem.Domain.User;
using FluentValidation;

namespace BookingSystem.Application.Features.Users.Commands.SignUp;

public class SignUpValidator : AbstractValidator<SignUpCommand>
{
    public SignUpValidator()
    {
        RuleFor(x => x.Password)
            .MinimumLength(8).MaximumLength(200);
    }
}