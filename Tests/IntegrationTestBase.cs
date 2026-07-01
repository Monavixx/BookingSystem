using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Tests.Fakes;
using Tests.Services;

namespace Tests;

[Collection("Postgres")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly PostgresTestFixture DbFixture;
    protected readonly IntegrationTestWebFactory Factory;
    protected IServiceScope Scope = null!;
    protected IMediator Mediator => Scope.ServiceProvider.GetRequiredService<IMediator>();
    protected AppDbContext DbContext => Scope.ServiceProvider.GetRequiredService<AppDbContext>();

    protected FakeCurrentUserService CurrentUserService =>
        (FakeCurrentUserService)Scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
    protected UserTestDataService Users => Scope.ServiceProvider.GetRequiredService<UserTestDataService>();
    protected RestaurantTestDataService Restaurants => Scope.ServiceProvider.GetRequiredService<RestaurantTestDataService>();
    
    protected IntegrationTestBase(PostgresTestFixture dbFixture)
    {
        DbFixture = dbFixture;
        Factory = new IntegrationTestWebFactory { ConnectionString = DbFixture.ConnectionString };
    }
    
    public Task InitializeAsync()
    {
        Scope = Factory.Services.CreateScope();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Scope.Dispose();
        await DbFixture.ResetDatabaseAsync();
    }
}

[CollectionDefinition("Postgres")]
public class PostgresCollection : ICollectionFixture<PostgresTestFixture>;