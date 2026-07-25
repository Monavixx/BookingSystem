using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Users.Errors;

public static class SessionErrors
{
    public static readonly UnauthorizedError NotFound =
        new("Session.NotFound", "The session was not found");
}