using System.Reflection;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Features.Bookings.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Application.Persistence.Extensions;
using BookingSystem.Infrastructure.Options;
using BookingSystem.Infrastructure.Services;
using BookingSystem.Infrastructure.Services.Cache;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BookingSystem.Infrastructure;

public static class DependencyInjection
{
    public static async Task AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RefreshTokenOptions>()
            .Bind(configuration.GetSection(RefreshTokenOptions.SectionName))
            .ValidateOnStart();
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ConstraintErrorRegistryBase>(sp =>
            {
                var factory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();

                using var db = factory.CreateDbContext();

                var cer = new ConstraintErrorRegistry(db.Model);
                cer.AddConstraintErrorsFromAssembly(typeof(AppDbContext).Assembly);

                return cer;
            }
        );


        if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
        {
            var hangfireConnection = configuration.GetConnectionString("HangfireConnection");
            if (!string.IsNullOrEmpty(hangfireConnection))
            {
                services.AddHangfire(c => c
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(a =>
                        a.UseNpgsqlConnection(hangfireConnection))
                );
                services.AddHangfireServer(c =>
                {
                    c.WorkerCount = 2;
                });
                services.AddScoped<IBackgroundJobService, BackgroundJobService>();
            }
        }
        string redisConnectionString = configuration.GetConnectionString("Redis") ?? string.Empty;
        IConnectionMultiplexer? connectionMultiplexer = null;
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            var conf = ConfigurationOptions.Parse(redisConnectionString);
            conf.AbortOnConnectFail = true;
            connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(conf);
            services.AddSingleton(connectionMultiplexer);
            services.AddSingleton<IUserCache, RedisUserCache>();
        }

        services.AddScoped<IBookingCancellationService, BookingCancellationService>();
        services.AddScoped<IBookingCompletionService, BookingCompletionService>();
        services.AddScoped<IUserBlocker, UserBlocker>();
        services.AddScoped<IUserStore, UserStore>();
    }
}
