using FluentResults;

namespace BookingSystem.Domain.Common.Errors;

public abstract class DomainError(string code, string message) : Error(message)
{
    public string Code { get; init; } = code;
    public abstract string Title { get; }
}