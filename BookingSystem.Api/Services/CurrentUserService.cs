using System.Security.Claims;
using BookingSystem.Api.Extensions;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.User;
using BookingSystem.Domain.User.ValueObjects;

namespace BookingSystem.Api.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext) : ICurrentUserService
{
    public Guid? UserIdGuid => httpContextAccessor.HttpContext?.User.GetUserIdOrDefault();
    public UserId? UserId => UserIdGuid is null ? null : new UserId(UserIdGuid.Value);

    public Guid GetRequiredUserIdGuid()
        => httpContextAccessor.HttpContext!.User.GetUserId();
    public UserId GetRequiredUserId()
        => new (GetRequiredUserIdGuid());

    private User? _user;
    public async ValueTask<User?> GetUserAsync()
    {
        if(_user is not null) return _user;
        if(UserId is not {} userId) return null;
        return _user = await dbContext.Users.FindAsync(userId);
    }

    public async ValueTask<User?> GetUserAsync(ClaimsPrincipal principal)
    {
        if (_user is not null) return _user;
        if (principal.GetUserIdOrDefault() is not { } userIdGuid) return null;
        return _user = await dbContext.Users.FindAsync(new UserId(userIdGuid));
    }
}