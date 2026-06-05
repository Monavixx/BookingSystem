using BookingSystem.Domain.Common.ValueObjects;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.ValueObjects;
using FluentValidation;

namespace BookingSystem.Application.Features.Auth.Commands.LogIn;

public class LogInValidator : AbstractValidator<LogInCommand>
{
    public LogInValidator()
    {
        Unless(u => string.IsNullOrEmpty(u.Email) && string.IsNullOrEmpty(u.Username), () =>
        {
            RuleFor(u => u.Username)
                .MinimumLength(Username.MinLength)
                .MaximumLength(Username.MaxLength)
                .When(u => string.IsNullOrEmpty(u.Email));
            RuleFor(u => u.Email)
                .NotEmpty()
                .MaximumLength(EmailAddress.MaxLength)
                .When(u => string.IsNullOrEmpty(u.Username));
        }).Otherwise(() =>
            RuleFor(u => u)
                .Must(_ => false)
                .WithMessage("Either Email or Username must be provided.")
        );
        
        RuleFor(u => u.Password)
            .NotEmpty()
            .MinimumLength(User.PasswordMinLength)
            .MaximumLength(User.PasswordMaxLength);
    }
}