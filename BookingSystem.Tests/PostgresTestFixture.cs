using BookingSystem.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace BookingSystem.Tests;

public class PostgresTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = 
        new PostgreSqlBuilder("postgres:latest").Build();

    public string ConnectionString => $"{_container.GetConnectionString()};Include Error Detail=true;";
    private Respawner _respawner = null!;
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        await using var factory = new IntegrationTestWebFactory(){ConnectionString = ConnectionString};
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        
        await using var connection = new NpgsqlConnection(ConnectionString);
        scope.ServiceProvider.GetRequiredService<ILogger<PostgresTestFixture>>().LogCritical(ConnectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection); 
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}