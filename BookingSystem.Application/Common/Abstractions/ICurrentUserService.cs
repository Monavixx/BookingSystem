namespace BookingSystem.Application.Common.Abstractions;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid GetRequiredUserId();
}