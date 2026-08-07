using BookingSystem.Domain.Users;

namespace BookingSystem.Application.Features.Users.DTOs;

public class CachedUser
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public DateTimeOffset RegistrationDateTime { get; set; }
    public DateOnly BirthDate { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.Guest;
    public bool IsBlocked { get; set; }
    public DateTimeOffset? BlockedUntil { get; set; }

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
