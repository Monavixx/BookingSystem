using System.Text.RegularExpressions;
using BookingSystem.Domain.Users.Errors;
using FluentResults;

namespace BookingSystem.Domain.Users.ValueObjects;

public record Username
{
    public const int MinLength = 3;
    public const int MaxLength = 100;
    private Username() { }
    public string Value { get; private init; } = null!;
    
    private static readonly Regex UsernameRegex = new(@"^[a-zA-Z0-9_\.]+$", RegexOptions.Compiled);

    public void Deconstruct(out string username)
    {
        username = Value;
    }

    public static Result<Username> Create(string username)
    {
        if(string.IsNullOrWhiteSpace(username))
            return Result.Fail<Username>(UserErrors.Username.Empty);
        if(username.Length > MaxLength)
            return Result.Fail<Username>(UserErrors.Username.TooLong);
        if(username.Length < MinLength)
            return Result.Fail<Username>(UserErrors.Username.TooShort);
        if(!UsernameRegex.IsMatch(username))
            return Result.Fail<Username>(UserErrors.Username.InvalidFormat);
        
        return new Username { Value = username };
    }
}