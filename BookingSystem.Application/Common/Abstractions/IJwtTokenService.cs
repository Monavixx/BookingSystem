using System.Security.Claims;
using BookingSystem.Domain.User;

namespace BookingSystem.Application.Common.Abstractions;

public interface IJwtTokenService
{
    string GenerateJwtToken(User user);
}