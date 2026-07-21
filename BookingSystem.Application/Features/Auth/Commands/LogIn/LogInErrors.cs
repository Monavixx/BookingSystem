using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Application.Features.Auth.Commands.LogIn;

public static class LogInErrors
{
    public static readonly DomainError InvalidCredentials =
        new UnauthorizedError("LogIn.InvalidCredentials", "Invalid username or password");

    public static readonly DomainError IdentifierAmbiguous =
        new ValidationError("LogIn.IdentifierAmbiguous", "Provide either Email or Username, not both");

    public static readonly DomainError IdentifierMissing =
        new ValidationError("LogIn.IdentifierMissing", "Email or Username is required");
}