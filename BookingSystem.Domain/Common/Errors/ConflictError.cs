namespace BookingSystem.Domain.Common.Errors;

public class ConflictError(string code, string message) : DomainError(code, message)
{
    public override string Title => "Conflict";

    public override DomainError CloneWithMessage(string message)
        => new ConflictError(Code, message);
}