using System.Security.Cryptography;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Domain.User.ValueObjects;
using BookingSystem.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace BookingSystem.Infrastructure.Services;

public class RefreshTokenService (IOptions<RefreshTokenOptions> options) : IRefreshTokenService
{
    public RefreshToken GenerateRefreshToken()
    {
        return RefreshToken.Create(RandomNumberGenerator.GetBytes(RefreshToken.TokenLength),
            DateTime.UtcNow.AddDays(options.Value.ExpirationDays));
    }
}