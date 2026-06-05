namespace BookingSystem.Domain.Users.ValueObjects;

public readonly record struct RegistrationDateTime(DateTime Value)
{
    public static RegistrationDateTime New() => new() { Value = DateTime.UtcNow };
}