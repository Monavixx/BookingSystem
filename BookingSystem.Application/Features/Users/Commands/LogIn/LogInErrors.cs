using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Application.Features.Users.Commands.LogIn;

public static class LogInErrors
{
    public static readonly UnauthorizedError InvalidCredentials =
        new UnauthorizedError("LogIn.InvalidCredentials", "Invalid username or password");

    public static readonly ValidationError IdentifierAmbiguous =
        new ValidationError("LogIn.IdentifierAmbiguous", "Provide either Email or Username, not both");

    public static readonly ValidationError IdentifierMissing =
        new ValidationError("LogIn.IdentifierMissing", "Email or Username is required");
}