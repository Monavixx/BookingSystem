using System.ComponentModel.DataAnnotations;
using BookingSystem.Domain.Common.ValueObjects.Errors;
using FluentResults;

namespace BookingSystem.Domain.Common.ValueObjects;

public sealed record PhoneNumber
{
    public const int MaxLength = 40;
    private PhoneNumber() { }
    private static readonly PhoneAttribute PhoneValidator = new();
    public static Result<PhoneNumber> Create(string phoneNumber)
    {
        if(string.IsNullOrWhiteSpace(phoneNumber))
            return Result.Fail<PhoneNumber>(PhoneNumberErrors.Empty);
        if(phoneNumber.Length > MaxLength)
            return Result.Fail<PhoneNumber>(PhoneNumberErrors.TooLong);
        if(!PhoneValidator.IsValid(phoneNumber))
            return Result.Fail<PhoneNumber>(PhoneNumberErrors.InvalidFormat);
        return new PhoneNumber { Value = phoneNumber };
    }

    public string Value { get; private init; } = null!;

    public void Deconstruct(out string phoneNumber)
    {
        phoneNumber = Value;
    }
}