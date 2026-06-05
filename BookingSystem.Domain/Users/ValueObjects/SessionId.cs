namespace BookingSystem.Domain.Users.ValueObjects;

public readonly record struct SessionId(Guid Value)
{
    public static SessionId New() => new(Guid.CreateVersion7());
}