using BookingSystem.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace BookingSystem.Tests;

public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer =
        new PostgreSqlBuilder("postgres:latest").Build();
    private readonly RedisContainer _redisContainer =
        new RedisBuilder("redis:latest").Build();

    public string PostgresConnectionString => $"{_postgresContainer.GetConnectionString()};Include Error Detail=true;";
    private Respawner _respawner = null!;

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_redisContainer.StartAsync(), _postgresContainer.StartAsync());

        await using var factory = new IntegrationTestWebFactory() { PostgresConnectionString = PostgresConnectionString, RedisConnectionString = _redisContainer.GetConnectionString() };
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        await using var connection = new NpgsqlConnection(PostgresConnectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(PostgresConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(Task.WhenAll(
                _postgresContainer.DisposeAsync().AsTask(),
                _redisContainer.DisposeAsync().AsTask()
                ));
    }
}
