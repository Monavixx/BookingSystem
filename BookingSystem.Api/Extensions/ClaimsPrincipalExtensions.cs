using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BookingSystem.Domain.User.ValueObjects;

namespace BookingSystem.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserIdOrDefault(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return null;
        return userId;
    }
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        if (principal is null)
            throw new ArgumentNullException(nameof(principal), $"{nameof(principal)} cannot be null.");
        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
        if(userIdClaim is null)
            throw new InvalidOperationException("User ID claim is missing." + string.Join(", ", principal.Claims.Select(c => $"{c.Type}: {c.Value}")));
        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            throw new InvalidOperationException("User ID claim is invalid.");
        return userId;
    }
}