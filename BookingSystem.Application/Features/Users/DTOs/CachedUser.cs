namespace BookingSystem.Application.Features.Users.DTOs;

public class CachedUser
{
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public byte[] PasswordHash { get; private set; } = null!;
    public DateTimeOffset RegistrationDateTime { get; private set; }
    public Birthdate BirthDate { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public UserRole Role { get; private set; } = UserRole.Guest;
    public bool IsBlocked { get; private set; }
    public DateTimeOffset? BlockedUntil { get; private set; }
}
