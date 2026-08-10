using System.Security.Cryptography;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Domain.Users.ValueObjects;
using BookingSystem.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace BookingSystem.Infrastructure.Services;

public class RefreshTokenService(IOptions<RefreshTokenOptions> options, TimeProvider timeProvider) : IRefreshTokenService
{
    public RefreshToken GenerateRefreshToken()
    {
        return RefreshToken.Create(RandomNumberGenerator.GetBytes(RefreshToken.TokenLength),
            timeProvider.GetUtcNow().UtcDateTime.AddDays(options.Value.ExpirationDays));
    }
}
