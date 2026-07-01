using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.Fakes;
using Tests.Services;

namespace Tests;

public class IntegrationTestWebFactory : WebApplicationFactory<Program>
{
    public string ConnectionString { get; init; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(ConnectionString));
            services.Replace(ServiceDescriptor.Scoped<ICurrentUserService, FakeCurrentUserService>());
            services.AddScoped<UserTestDataService>();
            services.AddScoped<RestaurantTestDataService>();
        });
    }
}