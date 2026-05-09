namespace BookingSystem.Domain.Common.Errors;

public class InternalServerError(string code, string message) : DomainError(code, message)
{
    public override string Title => "Internal Server Error";
}