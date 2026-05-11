using FluentResults;

namespace BookingSystem.Domain.Common.Errors;

public abstract class DomainError(string code, string message) : Error(message)
{
    public string Code { get; } = code;
    public abstract string Title { get; }
}