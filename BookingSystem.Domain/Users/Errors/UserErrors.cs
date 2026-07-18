using BookingSystem.Domain.Common.Errors;
using BookingSystem.Domain.Common.ValueObjects;

namespace BookingSystem.Domain.Users.Errors;

public static class UserErrors
{
    public static readonly DomainError IsBlocked = new ForbiddenError("User.UserIsBlocked", "User is blocked");
    public static readonly NotFoundError NotFound = new("User.NotFound", "User not found");
    public static readonly DomainError AdminCannotBeBlocked = new ForbiddenError("User.AdminCannotBeBlocked", "You can't block an admin");
    public static class Birthdate
    {
        public static readonly ValidationError TooYoung = new ("User.Birthdate.TooYoung",
            $"User must be at least {ValueObjects.Birthdate.MinAge} years old");
        public static readonly ValidationError TooOld = new ("User.Birthdate.TooOld",
            $"User cannot be older than {ValueObjects.Birthdate.MaxAge} years");
    }
    public static class Username
    {
        public static readonly ValidationError Empty = new("User.Username.Empty", "Username cannot be empty");
        public static readonly ValidationError TooLong = new("User.Username.TooLong",
            $"Username cannot be longer than {ValueObjects.Username.MaxLength} characters");
        public static readonly ValidationError TooShort = new("User.Username.TooShort",
            $"Username cannot be shorter than {ValueObjects.Username.MinLength} characters");
        public static readonly ValidationError InvalidFormat = new("User.Username.InvalidFormat",
            "Username can only contain letters, numbers, underscores and dots");
        public static readonly ConflictError AlreadyInUse = new("User.Username.AlreadyInUse",
            "Username is already in use");
    }

    public static class Email
    {
        public static readonly ValidationError Empty =
            new("User.Email.Empty", "Email cannot be empty");
        public static readonly ValidationError InvalidFormat =
            new("User.Email.InvalidFormat", "Invalid email format");
        public static readonly ValidationError TooLong =
            new("User.Email.TooLong", $"Email cannot be longer than {EmailAddress.MaxLength} characters");
        public static readonly ConflictError AlreadyInUse = new("User.Email.AlreadyInUse",
            "Email is already in use");
    }
    public static class PhoneNumber
    {
        public static readonly ValidationError InvalidFormat =
            new("User.PhoneNumber.InvalidFormat", "Invalid phone number format");
        public static readonly ValidationError Empty = new("User.PhoneNumber.Empty", "Phone number cannot be empty");
        public static readonly ValidationError TooLong = new("User.PhoneNumber.TooLong",
            $"Phone number cannot be longer than {Common.ValueObjects.PhoneNumber.MaxLength} characters");
        public static readonly ConflictError AlreadyInUse = new("User.PhoneNumber.AlreadyInUse",
            "Phone number is already in use");
    }

    public static class FirstName
    {
        public static readonly ValidationError Empty = new("User.FirstName.Empty", "First name cannot be empty");
        public static readonly ValidationError TooLong = new("User.FirstName.TooLong",
            $"First name cannot be longer than {User.FirstNameMaxLength} characters");
    }

    public static class LastName
    {
        public static readonly ValidationError Empty = new("User.LastName.Empty", "Last name cannot be empty");
        public static readonly ValidationError TooLong = new("User.LastName.TooLong",
            $"Last name cannot be longer than {User.LastNameMaxLength} characters");
    }

    public static class PasswordHash
    {
        public static readonly ValidationError Empty = new("User.PasswordHash.Empty", "Password hash cannot be empty");
    }
}