using BookingSystem.Application.Features.Users.DTOs;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Application.Common.Abstractions;

public interface IUserStore
{
    Task<CachedUser?> FindReadOnly(UserId id);
}
