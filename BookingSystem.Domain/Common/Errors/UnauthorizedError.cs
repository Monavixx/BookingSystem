using FluentResults;

namespace BookingSystem.Domain.Common.Errors;

public class UnauthorizedError(string code, string message) : DomainError(code, message)
{
    public override string Title => "Unauthorized";
    public override DomainError CloneWithMessage(string message)
        => new UnauthorizedError(Code, message);
}