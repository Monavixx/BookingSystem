using BookingSystem.Domain.Common;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Domain.Users;

public sealed class Session : Entity<SessionId>
{
    public UserId UserId { get; private set; }
    public RefreshToken RefreshToken { get; private set; } = null!;
    public User? User { get; private set; }

    public static Session Create(UserId userId, RefreshToken refreshToken)
        => new()
        {
            Id = SessionId.New(),
            UserId = userId,
            RefreshToken = refreshToken
        };

    public void UpdateRefreshToken(RefreshToken refreshToken)
        => RefreshToken = refreshToken;
}