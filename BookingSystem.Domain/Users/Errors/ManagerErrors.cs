using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Users.Errors;

public static class ManagerErrors
{
    public static readonly DomainError NotFound = new NotFoundError("Manager.NotFound", "The manager was not found");
}