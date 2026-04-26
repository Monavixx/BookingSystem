using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Common.ValueObjects.Errors;

public static class PhoneNumberErrors
{
    public static readonly ValidationError InvalidFormat =
        new("PhoneNumber.InvalidFormat", "Invalid phone number format");

    public static readonly ValidationError Empty = new("PhoneNumber.Empty", "Phone number cannot be empty");

    public static readonly ValidationError TooLong = new("PhoneNumber.TooLong",
        $"Phone number cannot be longer than {PhoneNumber.MaxLength} characters");
}