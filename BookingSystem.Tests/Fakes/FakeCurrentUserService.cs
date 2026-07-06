using System.Security.Claims;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Tests.Fakes;

public class FakeCurrentUserService (AppDbContext dbContext) : ICurrentUserService
{
    public Guid? UserIdGuid { get; set; }
    public UserId? UserId => new UserId(UserIdGuid!.Value);
    public Guid GetRequiredUserIdGuid() => UserIdGuid!.Value;

    public UserId GetRequiredUserId() => UserId!.Value;

    public ValueTask<User?> GetUserAsync()
    {
        return dbContext.Users.FindAsync(GetRequiredUserId());
    }

    public ValueTask<User?> GetUserAsync(ClaimsPrincipal principal)
    {
        throw new NotSupportedException();
    }
}