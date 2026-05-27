namespace BookingSystem.Domain.Common.Errors;

public class ForbiddenError(string code, string message) : DomainError(code, message)
{
    public override string Title => "Forbidden error";
}