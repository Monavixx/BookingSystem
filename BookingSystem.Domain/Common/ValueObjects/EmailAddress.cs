using System.ComponentModel.DataAnnotations;
using BookingSystem.Domain.Common.ValueObjects.Errors;
using FluentResults;

namespace BookingSystem.Domain.Common.ValueObjects;

public sealed record EmailAddress
{
    public const int MaxLength = 320;
    private EmailAddress() { }
    public string Value { get; private init; } = null!;

    public void Deconstruct(out string value)
    {
        value = Value;
    }
    
    private static readonly EmailAddressAttribute EmailValidator = new();

    public static Result<EmailAddress> Create(string email)
    {
        List<IError> errors = [];
        if (string.IsNullOrWhiteSpace(email))
            errors.Add(EmailAddressErrors.Empty);
        if(email.Length > MaxLength)
            errors.Add(EmailAddressErrors.TooLong);
        if(!EmailValidator.IsValid(email))
            errors.Add(EmailAddressErrors.InvalidFormat);
        if (errors.Count > 0) return Result.Fail<EmailAddress>(errors);
        return new EmailAddress { Value = email };
    }
}