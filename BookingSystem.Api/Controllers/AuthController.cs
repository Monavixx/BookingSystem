using BookingSystem.Api.Common;
using BookingSystem.Application.Features.Users.Commands.SignUp;
using BookingSystem.Infrastructure.Options;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BookingSystem.Api.Controllers;

[Route("api/auth")]
public class AuthController(IMediator mediator, IOptions<JwtOptions> jwtOptions) : ApiController(mediator)
{
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
    {
        var result = await Mediator.Send(command);
        
        if (!result.IsSuccess) return HandleErrors(result);
        
        HttpContext.Response.Cookies.Append("access_token", result.Value.AuthTokens.AccessToken, new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes)
        });
        HttpContext.Response.Cookies.Append("refresh_token", result.Value.AuthTokens.RefreshToken.ToString(),
            new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.Value.AuthTokens.RefreshToken.ExpiresAt
            });
        
        return CreatedAtAction(nameof(UsersController.GetCurrentUser),
            nameof(UsersController).Remove(nameof(UsersController).Length - 10), null, new{result.Value.Id});
    }
}