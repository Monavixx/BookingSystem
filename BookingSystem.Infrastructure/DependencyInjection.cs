using System.Reflection;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Application.Persistence.Extensions;
using BookingSystem.Infrastructure.Options;
using BookingSystem.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,  IConfiguration configuration)
    {
        services.AddOptions<RefreshTokenOptions>()
            .BindConfiguration(RefreshTokenOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ConstraintErrorRegistryBase, Services.ConstraintErrorRegistry>(sp =>
        {
            using var scope = sp.CreateScope();
            var cer = new Services.ConstraintErrorRegistry(scope.ServiceProvider.GetService<AppDbContext>()!.Model);
            cer.AddConstraintErrorsFromAssembly(typeof(AppDbContext).Assembly);
            return cer;
        });

        services.AddHangfire(c => c
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(a =>
                a.UseNpgsqlConnection(configuration.GetConnectionString("HangfireConnection")))
        );
        services.AddHangfireServer(c =>
        {
            c.WorkerCount = 2;
        });
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();

        return services;
    }
}