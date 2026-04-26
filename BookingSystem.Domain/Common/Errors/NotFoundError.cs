using FluentResults;

namespace BookingSystem.Domain.Common.Errors;

public class NotFoundError(string code, string message) : DomainError (code, message)
{
    public override string Title => "Not Found";
}