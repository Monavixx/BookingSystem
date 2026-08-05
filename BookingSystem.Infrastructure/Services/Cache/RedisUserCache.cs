using System.Text.Json;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.ValueObjects;
using StackExchange.Redis;

namespace BookingSystem.Infrastructure.Services.Cache;

public class RedisUserCache(IConnectionMultiplexer connectionMultiplexer) : IUserCache
{
    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(30);
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();
    private readonly JsonSerializerOptions _jsonOptions;

    public Task<User?> Find(UserId id)
    {
        throw new NotImplementedException();
    }

    public Task Invalidate(UserId id)
    {
        throw new NotImplementedException();
    }

    public Task Save(User user)
    {
        throw new NotImplementedException();
    }
}
