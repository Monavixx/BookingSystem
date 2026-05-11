using System.Reflection;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Application.Persistence.Extensions;
using BookingSystem.Infrastructure.Options;
using BookingSystem.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
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

        return services;
    }
}