namespace BookingSystem.Domain.Common.Errors;

public class ReferenceError(string code, string message) : DomainError(code, message)
{
    public override string Title => "Reference Error";
    public override DomainError CloneWithMessage(string message)
        => new ReferenceError(Code, message);
}