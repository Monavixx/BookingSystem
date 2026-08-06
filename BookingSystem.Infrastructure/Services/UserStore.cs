using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Features.Users.DTOs;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Users.ValueObjects;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

public class UserStore(IUserCache userCache, AppDbContext dbContext) : IUserStore
{
    public async Task<CachedUser?> FindReadOnly(UserId id)
    {
        var user = await userCache.Find(id);
        if (user is not null) return user;
        user = await dbContext.Database.GetDbConnection().QueryFirstOrDefaultAsync(
                """
                SELECT u.username, u.email, u.phone_number, u.registration_date_time, u.birth_date,
                u.first_name, u.last_name, u.role, u.is_blocked, u.blocked_until
                FROM users u
                WHERE u.id = @Id 
                LIMIT 1
                """, param: new { Id = id.Value });
        if (user is null) return null;
        await userCache.Save(user);
        return user;
    }
}
