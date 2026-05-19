using BookingSystem.Api.Common;
using BookingSystem.Application.Common.DTOs;
using BookingSystem.Application.Features.Auth.Commands.LogIn;
using BookingSystem.Application.Features.Auth.Commands.SignUp;
using BookingSystem.Infrastructure.Options;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BookingSystem.Api.Controllers;

[Route("api/auth")]
public class AuthController(IMediator mediator, IOptions<JwtOptions> jwtOptions) : ApiController(mediator)
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
    {
        var result = await Mediator.Send(command);
        if (result.IsFailed) return HandleErrors(result);
        
        AddAuthTokensToCookie(result.Value.AuthTokens);

        return CreatedAtAction(nameof(UsersController.GetCurrentUser),
            nameof(UsersController).Remove(nameof(UsersController).Length - 10), null, new { result.Value.Id });
    }

    [HttpPost("log-in")]
    public async Task<IActionResult> LogIn([FromBody] LogInCommand command)
    {
        var result = await Mediator.Send(command);
        if (result.IsFailed) return HandleErrors(result);
        
        AddAuthTokensToCookie(result.Value.AuthTokens);
        return Ok(new { result.Value.Id });
    }
    

    private void AddAuthTokensToCookie(AuthTokens authTokens)
    {
        HttpContext.Response.Cookies.Append("access_token", authTokens.AccessToken, new CookieOptions()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes)
        });

        HttpContext.Response.Cookies.Append("refresh_token", authTokens.RefreshToken.ToString(),
            new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = authTokens.RefreshToken.ExpiresAt
            });
    }
}