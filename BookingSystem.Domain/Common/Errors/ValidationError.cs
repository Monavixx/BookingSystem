namespace BookingSystem.Domain.Common.Errors;

public class ValidationError(string code, string message) : DomainError(code, message)
{
    public override string Title => "Validation Error";
    public override DomainError CloneWithMessage(string message)
        => new ValidationError(Code, message);
}