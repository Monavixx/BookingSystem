using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Common.ValueObjects.Errors;

public static class EmailAddressErrors
{
    public static readonly ValidationError Empty =
        new("Email.Empty", "Email cannot be empty");
    public static readonly ValidationError InvalidFormat =
        new("Email.InvalidFormat", "Invalid email format");
    public static readonly ValidationError TooLong =
        new("Email.TooLong", $"Email cannot be longer than {EmailAddress.MaxLength} characters");
}