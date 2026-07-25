using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Users.Errors;

public static class RefreshTokenErrors
{
    public static readonly DomainError Invalid = new ValidationError("RefreshToken.Invalid",
        "The provided refresh token is invalid");
}