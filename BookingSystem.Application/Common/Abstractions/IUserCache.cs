using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Application.Common.Abstractions;

public interface IUserCache
{
    Task<User?> Find(UserId id);
    Task Invalidate(UserId id);
    Task Save(User user);
}
