using BookingSystem.Application.Features.Users.DTOs;
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
    public static UserDto FromUser(CachedUser user)
        => new(
            Id: user.Id,
            Username: user.Username,
            Email: user.Email,
            PhoneNumber: user.PhoneNumber,
            RegistrationDateTime: user.RegistrationDateTime,
            BirthDate: user.BirthDate,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Role: user.Role.ToString(),
            IsBlocked: user.IsBlocked,
            BlockedUntil: user.BlockedUntil
        );
}
