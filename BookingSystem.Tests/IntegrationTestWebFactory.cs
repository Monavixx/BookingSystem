using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Tests.Fakes;
using BookingSystem.Tests.Services;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookingSystem.Tests;

public class IntegrationTestWebFactory : WebApplicationFactory<Program>
{
    public string ConnectionString { get; init; } = null!;
    private Mock<IBackgroundJobService> _backgroundJobServiceMock = new ();

    public Mock<IBackgroundJobService> GetBackgroundJobServiceMock()
    {
        if (_backgroundJobServiceMock == null)
            throw new InvalidOperationException("BackgroundJobServiceMock has not been initialized.");
        return _backgroundJobServiceMock;
    }

    private IBackgroundJobService GetBackgroundJobService() => _backgroundJobServiceMock!.Object;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });
        builder.ConfigureServices(services =>
        {
            var hangfireServer =
                services.SingleOrDefault(d =>
                    d.ServiceType == typeof(IHostedService)
                    && d.ImplementationType == typeof(BackgroundJobServerHostedService));
            if (hangfireServer is not null) services.Remove(hangfireServer);
            
            var backgroundJobServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IBackgroundJobService));
            if (backgroundJobServiceDescriptor is not null) services.Remove(backgroundJobServiceDescriptor);
            
            var descriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(AppDbContext) ||
                    d.ServiceType == typeof(IDbContextFactory<AppDbContext>) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>)))
                .ToList();

            foreach (var d in descriptors)
                services.Remove(d);

            services.AddDbContextFactory<AppDbContext>(options => { 
                options.UseNpgsql(ConnectionString)
                    .UseSnakeCaseNamingConvention();
                options.ConfigureWarnings(c => c.Ignore(RelationalEventId.PendingModelChangesWarning));
            });
            services.Replace(ServiceDescriptor.Scoped<ICurrentUserService, FakeCurrentUserService>());
            services.AddScoped<UserTestDataService>();
            services.AddScoped<RestaurantTestDataService>();
            services.AddScoped<BookingTestDataService>();
            
            // Register mock IBackgroundJobService
            services.AddScoped<IBackgroundJobService>(_ => GetBackgroundJobService());
        });
    }
}