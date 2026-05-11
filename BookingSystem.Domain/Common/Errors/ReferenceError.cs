namespace BookingSystem.Domain.Common.Errors;

public class ReferenceError(string code, string message) : DomainError(code, message)
{
    public override string Title => "Reference Error";
}