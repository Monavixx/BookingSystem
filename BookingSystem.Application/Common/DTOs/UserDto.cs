using BookingSystem.Domain.Users;

namespace BookingSystem.Application.Common.DTOs;

public sealed record UserDto(
    Guid Id,
    string Username,
    string Email,
    string PhoneNumber,
    DateTimeOffset RegistrationDateTime,
    DateOnly BirthDate,
    string FirstName,
    string LastName,
    string Role
)
{
    public static UserDto FromUser(User user)
        => new (
            Id: user.Id.Value,
            Username : user.Username.Value,
            Email: user.Email.Value,
            PhoneNumber: user.PhoneNumber.Value,
            RegistrationDateTime: user.RegistrationDateTime,
            BirthDate: user.BirthDate.Value,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Role: user.Role.ToString()
        );
}