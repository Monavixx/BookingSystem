using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Domain.Users;
using BookingSystem.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace BookingSystem.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly TimeProvider _timeProvider;
    private readonly JwtOptions _options;
    private readonly SymmetricSecurityKey _key;
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    public JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _options = options.Value;
        _key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_options.Secret));
    }

    public string GenerateJwtToken(User user)
    {
        var claims = GenerateClaims(user);
        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(_options.ExpirationMinutes),
            SigningCredentials = credentials,
            Issuer = _options.Issuer,
            Audience = _options.Audience
        };
        var token = TokenHandler.CreateToken(tokenDescriptor);
        return TokenHandler.WriteToken(token);
    }

    private static IEnumerable<Claim> GenerateClaims(User user)
        =>
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        ];
}
