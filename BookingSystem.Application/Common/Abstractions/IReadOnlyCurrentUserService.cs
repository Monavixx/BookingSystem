using System.Security.Claims;
using BookingSystem.Application.Features.Users.DTOs;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Application.Common.Abstractions;

public interface IReadOnlyCurrentUserService
{
    ValueTask<CachedUser?> GetAsync(ClaimsPrincipal principal);
    ValueTask<CachedUser?> GetAsync();
    Guid? UserIdGuid { get; }
    UserId? UserId { get; }
    Guid GetRequiredUserIdGuid();
    UserId GetRequiredUserId();
}
