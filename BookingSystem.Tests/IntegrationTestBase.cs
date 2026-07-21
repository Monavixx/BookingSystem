using System.Collections.Concurrent;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.ValueObjects;
using BookingSystem.Tests.Fakes;
using BookingSystem.Tests.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace BookingSystem.Tests;

[Collection("Postgres")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly PostgresTestFixture DbFixture;
    protected readonly IntegrationTestWebFactory Factory;
    protected IServiceScope Scope = null!;
    protected IMediator Mediator => Scope.ServiceProvider.GetRequiredService<IMediator>();
    protected AppDbContext DbContext => Scope.ServiceProvider.GetRequiredService<AppDbContext>();
    protected FakeTimeProvider FakeTime => Factory.FakeTime;
    private IDbContextFactory<AppDbContext> DbContextFactory => Scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    private readonly ConcurrentBag<AppDbContext> _dbContextsToDispose = [];
    protected AppDbContext NewDbContext()
    {
        var dbCtx = DbContextFactory.CreateDbContext();
        _dbContextsToDispose.Add(dbCtx);
        return dbCtx;
    }

    protected FakeCurrentUserService CurrentUserService =>
        (FakeCurrentUserService)Scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
    protected UserTestDataService Users => Scope.ServiceProvider.GetRequiredService<UserTestDataService>();
    protected RestaurantTestDataService Restaurants => Scope.ServiceProvider.GetRequiredService<RestaurantTestDataService>();
    protected BookingTestDataService Bookings => Scope.ServiceProvider.GetRequiredService<BookingTestDataService>();
    protected Mock<IBackgroundJobService> BackgroundJobServiceMock => Factory.GetBackgroundJobServiceMock();
    
    protected IntegrationTestBase(PostgresTestFixture dbFixture)
    {
        DbFixture = dbFixture;
        Factory = new IntegrationTestWebFactory { ConnectionString = DbFixture.ConnectionString };
    }

    protected void SetCurrentUser(User user) => CurrentUserService.UserIdGuid = user.Id.Value;
    protected void SetCurrentUser(Guid id) => CurrentUserService.UserIdGuid = id;
    protected void SetCurrentUser(UserId id) => CurrentUserService.UserIdGuid = id.Value;

    protected IServiceScope NewScope()
    {
        var userId = CurrentUserService.UserIdGuid;
        Scope.Dispose();
        Scope = Factory.Services.CreateScope();
        CurrentUserService.UserIdGuid = userId;
        return Scope;
    }

    protected virtual ValueTask InitAsync()
    {
        return ValueTask.CompletedTask;
    }
    public ValueTask InitializeAsync()
    {
        _dbContextsToDispose.Clear();
        Scope = Factory.Services.CreateScope();
        // Reset the mock before each test
        BackgroundJobServiceMock.Reset();
        return InitAsync();
    }

    public async ValueTask DisposeAsync()
    {
        Scope.Dispose();
        await Task.WhenAll(_dbContextsToDispose.Select(c => c.DisposeAsync().AsTask()));
        await DbFixture.ResetDatabaseAsync();
    }
}

[CollectionDefinition("Postgres")]
public class PostgresCollection : ICollectionFixture<PostgresTestFixture>;