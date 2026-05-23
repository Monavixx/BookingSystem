using System.Security.Claims;
using BookingSystem.Domain.User;
using BookingSystem.Domain.User.ValueObjects;

namespace BookingSystem.Application.Common.Abstractions;

public interface ICurrentUserService
{
    Guid? UserIdGuid { get; }
    UserId? UserId { get; }
    Guid GetRequiredUserIdGuid();
    UserId GetRequiredUserId();
    ValueTask<User?> GetUserAsync();
    ValueTask<User?> GetUserAsync(ClaimsPrincipal principal);
}