using BookingSystem.Domain.Common.Errors;
using FluentResults;

namespace BookingSystem.Domain.User.ValueObjects;

public readonly record struct RegistrationDateTime(DateTime Value)
{
    public static RegistrationDateTime New() => new() { Value = DateTime.UtcNow };
}