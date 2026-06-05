using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Users.Errors;

public static class SessionErrors
{
    public static readonly UnauthorizedError InvalidRefreshToken =
        new("Session.InvalidRefreshToken", "The provided refresh token is invalid.");
}