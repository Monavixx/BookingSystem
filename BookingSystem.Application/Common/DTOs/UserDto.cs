using BookingSystem.Domain.Users;
using JetBrains.Annotations;

namespace BookingSystem.Application.Common.DTOs;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record UserDto(
    Guid Id,
    string Username,
    string Email,
    string PhoneNumber,
    DateTimeOffset RegistrationDateTime,
    DateOnly BirthDate,
    string FirstName,
    string LastName,
    string Role,
    bool IsBlocked,
    DateTimeOffset? BlockedUntil = null
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
            Role: user.Role.ToString(),
            IsBlocked: user.IsBlocked,
            BlockedUntil: user.BlockedUntil
        );
}