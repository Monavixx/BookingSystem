using BookingSystem.Domain.Common.ValueObjects;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Domain.Users;

public sealed partial class User
{
    /// <summary>
    /// This constructor is intended for testing purposes only.
    /// It allows the creation of a User instance with specific values for all properties,
    /// bypassing any validation or business rules that would normally be enforced
    /// in the public constructors or factory methods. Use this constructor with caution,
    /// as it may lead to inconsistent or invalid state if used improperly.
    /// </summary>
    internal User(string username, string email, string phoneNumber, byte[] passwordHash, DateTimeOffset registrationDateTime, DateOnly birthDate, string firstName, string lastName, UserRole role, bool isBlocked, DateTimeOffset? blockedUntil)
    {
        Id = UserId.New();
        Username = Username.Create(username).Value;
        Email = EmailAddress.Create(email).Value;
        PhoneNumber = PhoneNumber.Create(phoneNumber).Value;
        PasswordHash = passwordHash;
        RegistrationDateTime = registrationDateTime;
        BirthDate = Birthdate.__CreateUnchecked(birthDate);
        FirstName = firstName;
        LastName = lastName;
        Role = role;
        IsBlocked = isBlocked;
        BlockedUntil = blockedUntil;
    }
}