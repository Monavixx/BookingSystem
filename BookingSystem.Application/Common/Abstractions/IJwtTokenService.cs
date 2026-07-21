using BookingSystem.Domain.Users;

namespace BookingSystem.Application.Common.Abstractions;

public interface IJwtTokenService
{
    string GenerateJwtToken(User user);
}