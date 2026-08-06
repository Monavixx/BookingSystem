using System.Text.Json;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Features.Users.DTOs;
using BookingSystem.Domain.Users.ValueObjects;
using StackExchange.Redis;

namespace BookingSystem.Infrastructure.Services.Cache;

public class RedisUserCache(IConnectionMultiplexer connectionMultiplexer) : IUserCache
{
    private const string KeyPrefix = "user:";
    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(30);
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<CachedUser?> Find(UserId id)
    {
        var key = BuildKey(id);
        var value = await _database.StringGetAsync(key);

        if (!value.HasValue)
            return null;

        try
        {
            return JsonSerializer.Deserialize<CachedUser>(value.ToString(), _jsonOptions);
        }
        catch
        {
            await _database.KeyDeleteAsync(key);
            return null;
        }
    }

    public async Task Invalidate(UserId id)
    {
        var key = BuildKey(id);
        await _database.KeyDeleteAsync(key);
    }

    public async Task Save(CachedUser user)
    {
        var key = BuildKey(new UserId(user.Id));
        var json = JsonSerializer.Serialize(user, _jsonOptions);
        await _database.StringSetAsync(key, json, Expiration);
    }

    private static string BuildKey(UserId id)
        => $"{KeyPrefix}{id.Value:N}";
}
