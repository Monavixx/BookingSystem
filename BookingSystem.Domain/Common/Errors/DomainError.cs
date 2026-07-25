using FluentResults;

namespace BookingSystem.Domain.Common.Errors;

public abstract class DomainError(string code, string message) : Error(message)
{
    public string Code { get; } = code;
    public abstract string Title { get; }

    /// <summary>
    /// Clones the error with changed message. Does not affect the original DomainError.
    /// </summary>
    public abstract DomainError CloneWithMessage(string message);

    public override string ToString()
    => $"{nameof(Code)}: {Code}, {nameof(Title)}: {Title}, {nameof(Message)}: {Message}, {nameof(Metadata)}: " +
       $"{string.Join("; ", Metadata.Select(kv => $"({kv.Key}={kv.Value})"))}";
}