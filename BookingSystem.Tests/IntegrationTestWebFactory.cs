using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Infrastructure.Services;
using BookingSystem.Tests.Fakes;
using BookingSystem.Tests.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using StackExchange.Redis;

namespace BookingSystem.Tests;

public class IntegrationTestWebFactory : WebApplicationFactory<Program>
{
    public string PostgresConnectionString { get; init; } = null!;
    public string RedisConnectionString { get; init; } = null!;
    private readonly Mock<IBackgroundJobService> _backgroundJobServiceMock = new();

    public FakeTimeProvider FakeTime { get; init; } =
        new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

    public Mock<IBackgroundJobService> GetBackgroundJobServiceMock()
    {
        return _backgroundJobServiceMock;
    }

    private IBackgroundJobService GetBackgroundJobService() => _backgroundJobServiceMock.Object;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(RedisConnectionString)));

            services.Add(ServiceDescriptor.Scoped(_ => GetBackgroundJobService()));

            services.AddSingleton<TimeProvider>(FakeTime);

            services.AddDbContextFactory<AppDbContext>(options =>
            {
                options.UseNpgsql(PostgresConnectionString)
                    .UseSnakeCaseNamingConvention();
                // options.ConfigureWarnings(c => c.Ignore(RelationalEventId.PendingModelChangesWarning));
            });
            services.AddScoped<ICurrentUserService, FakeCurrentUserService>();
            services.AddScoped<IReadOnlyCurrentUserService, FakeReadOnlyCurrentUserService>();
            services.AddScoped<UserTestDataService>();
            services.AddScoped<RestaurantTestDataService>();
            services.AddScoped<BookingTestDataService>();
            services.AddScoped<UserBlocker>();
            services.AddScoped<BookingCancellationService>();
        });
    }
}
