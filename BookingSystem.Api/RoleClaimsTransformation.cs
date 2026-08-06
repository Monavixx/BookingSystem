using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BookingSystem.Application.Common.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace BookingSystem.Api;

public class RoleClaimsTransformation(IReadOnlyCurrentUserService readOnlyCurrentUserService) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is { IsAuthenticated: true })
        {
            var user = await readOnlyCurrentUserService.GetAsync(principal);
            if (user != null)
            {
                var id = new ClaimsIdentity([new Claim("role", user.Role.ToString())],
                    JwtBearerDefaults.AuthenticationScheme,
                    JwtRegisteredClaimNames.Sub, "role");
                principal.AddIdentity(id);
            }
        }

        return principal;
    }
}
