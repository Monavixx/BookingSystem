using BookingSystem.Api.Common;
using BookingSystem.Application.Common.DTOs;
using BookingSystem.Application.Features.Auth.Commands.LogIn;
using BookingSystem.Application.Features.Auth.Commands.Refresh;
using BookingSystem.Application.Features.Auth.Commands.SignUp;
using BookingSystem.Infrastructure.Options;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BookingSystem.Api.Controllers;

/// <summary>
/// Authentication controller for managing user authentication operations.
/// </summary>
/// <remarks>
/// Provides endpoints for user registration, login, and token refresh operations.
/// Uses cookie-based JWT token storage with HttpOnly flag for security.
/// </remarks>
[Route("api/auth")]
public class AuthController(IMediator mediator, IOptions<JwtOptions> jwtOptions, TimeProvider timeProvider) : ApiController(mediator)
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="command">The sign-up request containing user credentials and personal information.</param>
    /// <returns>
    /// A 201 Created response with the newly created user ID if successful.
    /// Returns 400 Bad Request for validation errors (empty/invalid fields, age requirements).
    /// Returns 409 Conflict if username, email, or phone number already exists.
    /// </returns>
    /// <response code="201">User successfully registered. Authentication cookies are set automatically.</response>
    /// <response code="400">Validation error (invalid email format, username too short/long, user too young/old, etc.).</response>
    /// <response code="409">Conflict: username, email, or phone number already registered.</response>
    [HttpPost("sign-up")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
    {
        var result = await Mediator.Send(command);
        if (result.IsFailed) return HandleErrors(result);

        AddAuthTokensToCookie(result.Value.AuthTokens);

        return CreatedAtAction(nameof(UsersController.GetCurrentUser),
            nameof(UsersController)[..^10], null, new { result.Value.Id });
    }

    /// <summary>
    /// Authenticates a user with credentials.
    /// </summary>
    /// <param name="command">The login request containing email/username and password.</param>
    /// <returns>
    /// A 200 OK response with the authenticated user ID if successful.
    /// Returns 400 Bad Request for validation errors.
    /// Returns 401 Unauthorized if credentials are invalid.
    /// Returns 404 Not Found if user does not exist.
    /// </returns>
    /// <response code="200">User successfully authenticated. Returns user ID and sets authentication cookies.</response>
    /// <response code="400">Validation error in login request.</response>
    /// <response code="401">Invalid credentials (wrong password or user is blocked).</response>
    /// <response code="404">User not found.</response>
    [HttpPost("log-in")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LogIn([FromBody] LogInCommand command)
    {
        var result = await Mediator.Send(command);
        if (result.IsFailed) return HandleErrors(result);

        AddAuthTokensToCookie(result.Value.AuthTokens);
        return Ok(new { result.Value.Id });
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <returns>
    /// A 204 No Content response if token refresh is successful.
    /// Returns 400 Bad Request for validation errors.
    /// Returns 401 Unauthorized if the refresh token is invalid or expired.
    /// </returns>
    /// <remarks>
    /// Reads the refresh token from the 'refresh_token' cookie and issues a new access token.
    /// The refresh token must be valid and not expired.
    /// </remarks>
    /// <response code="204">Token successfully refreshed. New access token is set in cookie.</response>
    /// <response code="400">Validation error in refresh request.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        var result = await Mediator.Send(new RefreshCommand(Request.Cookies["refresh_token"]!));
        if (result.IsFailed) return HandleErrors(result);

        AddAuthTokensToCookie(result.Value);
        return NoContent();
    }

    /// <summary>
    /// Adds authentication tokens to HTTP response cookies.
    /// </summary>
    /// <param name="authTokens">The access and refresh tokens to set as cookies.</param>
    /// <remarks>
    /// Sets two secure, HttpOnly cookies:
    /// - access_token: JWT token for API authorization (expires per JWT configuration)
    /// - refresh_token: Refresh token for obtaining new access tokens (expires per token expiration)
    /// Both cookies use Strict SameSite policy for enhanced security.
    /// </remarks>
    private void AddAuthTokensToCookie(AuthTokens authTokens)
    {
        HttpContext.Response.Cookies.Append("access_token", authTokens.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = timeProvider.GetUtcNow().AddMinutes(_jwtOptions.ExpirationMinutes)
        });

        HttpContext.Response.Cookies.Append("refresh_token", authTokens.RefreshToken.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = authTokens.RefreshToken.ExpiresAt
            });
    }
}
