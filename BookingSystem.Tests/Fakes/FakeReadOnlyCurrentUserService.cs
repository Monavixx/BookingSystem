using System.Security.Claims;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Features.Users.DTOs;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Tests.Fakes;

public class FakeReadOnlyCurrentUserService(IUserStore userStore) : IReadOnlyCurrentUserService
{
    public Guid? UserIdGuid { get; set; }
    public UserId? UserId => new UserId(UserIdGuid!.Value);
    public Guid GetRequiredUserIdGuid() => UserIdGuid!.Value;

    public UserId GetRequiredUserId() => UserId!.Value;

    public ValueTask<CachedUser?> GetAsync()
        => GetAsync(UserIdGuid);
    private CachedUser? _user = null;
    private async ValueTask<CachedUser?> GetAsync(Guid? id)
    {
        if (_user is not null) return _user;
        if (id is not { } idNotNull) return null;
        return _user = await userStore.FindReadOnly(new UserId(idNotNull));
    }

    public ValueTask<CachedUser?> GetAsync(ClaimsPrincipal principal)
    {
        throw new NotSupportedException();
    }
}
