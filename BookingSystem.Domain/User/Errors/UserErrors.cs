using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.User.Errors;

public static class UserErrors
{
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