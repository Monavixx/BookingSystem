using BookingSystem.Domain.Users.Errors;
using FluentResults;

namespace BookingSystem.Domain.Users.ValueObjects;

public sealed record Birthdate
{
    private Birthdate() { }
    public DateOnly Value { get; private init; }
    public const int MinAge = 16;
    public const int MaxAge = 150;

    public static Result<Birthdate> Create(TimeProvider timeProvider, DateOnly date)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        if (date > today.AddYears(-MinAge))
            return Result.Fail<Birthdate>(UserErrors.Birthdate.TooYoung);
        if(date < today.AddYears(-MaxAge))
            return Result.Fail<Birthdate>(UserErrors.Birthdate.TooOld);
        return new Birthdate { Value = date };
    }
    
    public static Birthdate __CreateUnchecked(DateOnly date) => new() { Value = date };
}