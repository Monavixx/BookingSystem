using BookingSystem.Domain.Users;

namespace BookingSystem.Application.Features.Users.DTOs;

public class CachedUser
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public DateTimeOffset RegistrationDateTime { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public UserRole Role { get; private set; } = UserRole.Guest;
    public bool IsBlocked { get; private set; }
    public DateTimeOffset? BlockedUntil { get; private set; }

    public static CachedUser FromUser(User user)
        => new()
        {
            Id = user.Id.Value,
            Username = user.Username.Value,
            Email = user.Email.Value,
            PhoneNumber = user.PhoneNumber.Value,
            RegistrationDateTime = user.RegistrationDateTime,
            BirthDate = user.BirthDate.Value,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            IsBlocked = user.IsBlocked,
            BlockedUntil = user.BlockedUntil
        };
}
