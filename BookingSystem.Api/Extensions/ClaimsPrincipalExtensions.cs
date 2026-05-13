using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BookingSystem.Domain.User.ValueObjects;

namespace BookingSystem.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserIdOrDefault(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return null;
        return userId;
    }
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new InvalidOperationException("User ID claim is missing or invalid.");
        return userId;
    }
}