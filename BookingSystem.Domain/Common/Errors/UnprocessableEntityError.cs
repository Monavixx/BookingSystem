namespace BookingSystem.Domain.Common.Errors;

public class UnprocessableEntityError(string code, string message) : DomainError(code, message)
{
    public override string Title => "Unprocessable Entity";
}