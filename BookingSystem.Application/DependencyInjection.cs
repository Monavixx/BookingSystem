using BookingSystem.Application.Common.Options;
using BookingSystem.Application.Common.PipelineBehaviors;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Application;

public static class DependencyInjection
{
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                    .UseSnakeCaseNamingConvention();
            });
            services.AddMediatR(c =>
            {
                c.RegisterServicesFromAssembly(typeof(AppDbContext).Assembly);
                c.AddOpenBehavior(typeof(ValidationBehavior<,>));
                c.AddOpenBehavior(typeof(ActiveUserCheckBehavior<,>));
                c.AddOpenBehavior(typeof(LoggingBehavior<,>));
                c.AddOpenBehavior(typeof(DbExceptionHandlingBehavior<,>));
            });
            services.AddSingleton<BookingDurationCalculator>();
            services.AddOptions<BookingOptions>()
                .Bind(configuration.GetSection(BookingOptions.SectionName))
                .ValidateOnStart();
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            return services;
        }
}