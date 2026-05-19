using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookingSystem.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserIdOrDefault(this ClaimsPrincipal principal)
    {
        if (principal is null)
            throw new ArgumentNullException(nameof(principal), $"{nameof(principal)} cannot be null.");
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
            throw new InvalidOperationException("User ID claim is missing.");
        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            throw new InvalidOperationException("User ID claim is invalid.");
        return userId;
    }
}