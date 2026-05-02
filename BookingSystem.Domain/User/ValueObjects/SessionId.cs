namespace BookingSystem.Domain.User.ValueObjects;

public readonly record struct SessionId(Guid Value)
{
    public static SessionId New() => new(Guid.CreateVersion7());
}