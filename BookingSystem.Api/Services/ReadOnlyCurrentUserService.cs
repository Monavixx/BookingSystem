using System.Security.Claims;
using BookingSystem.Api.Extensions;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Features.Users.DTOs;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Api.Services;

public class ReadOnlyCurrentUserService(IUserStore userStore, IHttpContextAccessor httpContextAccessor) : IReadOnlyCurrentUserService
{
    public Guid? UserIdGuid => httpContextAccessor?.HttpContext?.User.GetUserIdOrDefault();
    public UserId? UserId => UserIdGuid is null ? null : new UserId(UserIdGuid.Value);

    public Guid GetRequiredUserIdGuid()
        => httpContextAccessor.HttpContext!.User.GetUserId();
    public UserId GetRequiredUserId()
        => new(GetRequiredUserIdGuid());

    private CachedUser? _user = null;

    public ValueTask<CachedUser?> GetAsync(ClaimsPrincipal principal) => GetAsync(principal.GetUserIdOrDefault());
    public ValueTask<CachedUser?> GetAsync()
        => GetAsync(UserIdGuid);

    private async ValueTask<CachedUser?> GetAsync(Guid? id)
    {
        if (_user is not null) return _user;
        if (id is not { } idNotNull) return null;
        return _user = await userStore.FindReadOnly(new UserId(idNotNull));
    }
}
