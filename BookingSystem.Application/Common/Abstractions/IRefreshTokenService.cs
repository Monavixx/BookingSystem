using BookingSystem.Domain.User.ValueObjects;

namespace BookingSystem.Application.Common.Abstractions;

public interface IRefreshTokenService
{
    RefreshToken GenerateRefreshToken();
}