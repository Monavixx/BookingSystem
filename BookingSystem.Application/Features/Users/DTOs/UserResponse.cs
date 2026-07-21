using System.Linq.Expressions;
using BookingSystem.Domain.Users;

namespace BookingSystem.Application.Features.Users.DTOs;

public record UserResponse
{
    public UserResponse() { }
    public UserResponse(Guid Id,
        string Username,
        string PhoneNumber,
        string Email,
        string FirstName,
        string LastName,
        DateTimeOffset RegistrationDate,
        DateOnly BirthDate,
        UserRole Role,
        bool IsBlocked,
        DateTimeOffset? BlockedUntil)
    {
        this.Id = Id;
        this.Username = Username;
        this.PhoneNumber = PhoneNumber;
        this.Email = Email;
        this.FirstName = FirstName;
        this.LastName = LastName;
        this.RegistrationDate = RegistrationDate;
        this.BirthDate = BirthDate;
        this.Role = Role;
        this.IsBlocked = IsBlocked;
        this.BlockedUntil = BlockedUntil;
    }

    public static readonly Expression<Func<User, UserResponse>> Projection =
        u => new UserResponse
        {
            Id = u.Id.Value,
            Username = u.Username.Value,
            PhoneNumber = u.PhoneNumber.Value,
            Email = u.Email.Value,
            FirstName = u.FirstName,
            LastName = u.LastName,
            RegistrationDate = u.RegistrationDateTime,
            BirthDate = u.BirthDate.Value,
            Role = u.Role,
            IsBlocked = u.IsBlocked,
            BlockedUntil = u.BlockedUntil
        };

    public Guid Id { get; init; } = Guid.Empty;
    public string Username { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public DateTimeOffset RegistrationDate { get; init; }
    public DateOnly BirthDate { get; init; }
    public UserRole Role { get; init; }
    public bool IsBlocked { get; init; }
    public DateTimeOffset? BlockedUntil { get; init; }
}