using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.PipelineBehaviors;
using BookingSystem.Application.Persistence;
using BookingSystem.Application.Services;
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
                c.AddOpenBehavior(typeof(LoggingBehavior<,>));
                c.AddOpenBehavior(typeof(DbExceptionHandlingBehavior<,>));
            });
            services.AddScoped<IBookingCancellationService, BookingCancellationService>();
            services.AddSingleton<BookingDurationCalculator>();
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            return services;
        }
}