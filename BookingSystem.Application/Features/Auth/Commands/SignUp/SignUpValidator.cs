using BookingSystem.Domain.Common.ValueObjects;
using BookingSystem.Domain.User;
using BookingSystem.Domain.User.ValueObjects;
using FluentValidation;

namespace BookingSystem.Application.Features.Auth.Commands.SignUp;

public class SignUpValidator : AbstractValidator<SignUpCommand>
{
    public SignUpValidator()
    {
        RuleFor(x => x.Password)
            .MinimumLength(User.PasswordMinLength)
            .MaximumLength(User.PasswordMaxLength);
        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(EmailAddress.MaxLength);
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(Username.MinLength)
            .MaximumLength(Username.MaxLength);
        RuleFor(x => x.FirstName)
            .NotEmpty();
        RuleFor(x => x.LastName)
            .NotEmpty();
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(PhoneNumber.MaxLength);
    }
}