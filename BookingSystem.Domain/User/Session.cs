using BookingSystem.Domain.Common;
using BookingSystem.Domain.User.ValueObjects;

namespace BookingSystem.Domain.User;

public sealed class Session : Entity<SessionId>
{
    public UserId UserId { get; private set; }
    public RefreshToken RefreshToken { get; private set; } = null!;

    public static Session Create(UserId userId, RefreshToken refreshToken)
        => new Session()
        {
            Id = SessionId.New(),
            UserId = userId,
            RefreshToken = refreshToken
        };
}