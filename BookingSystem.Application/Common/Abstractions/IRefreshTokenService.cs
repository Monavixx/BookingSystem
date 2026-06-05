using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Application.Common.Abstractions;

public interface IRefreshTokenService
{
    RefreshToken GenerateRefreshToken();
}