using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using BookingSystem.Api;
using BookingSystem.Api.Middlewares;
using BookingSystem.Api.Services;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Infrastructure.Options;
using BookingSystem.Infrastructure.Services;
using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using BookingSystem.Application.Persistence.Extensions;
using System.Reflection;
using Hangfire.PostgreSql;
using StackExchange.Redis;
using BookingSystem.Infrastructure.Services.Cache;
using BookingSystem.Application.Features.Bookings.Abstractions;
using BookingSystem.Domain.Bookings.Services;
using BookingSystem.Application.Common.Options;
using BookingSystem.Application.Common.PipelineBehaviors;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(LogEventLevel.Debug)
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    // Disable legacy WS-Federation claim type mapping.
    // Without this, standard JWT claims like "sub" get remapped to long URI-based
    // claim types (e.g., sub → NameIdentifier URI), which breaks direct claim lookups.
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
    JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

    var builder = WebApplication.CreateBuilder(args);
    AddCoreServices(builder);


    if (!builder.Environment.IsEnvironment("Test"))
        await AddInfrastructureServices(builder);

    var app = builder.Build();


    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapHangfireDashboard();
        app.MapScalarApiReference();
    }

    app.UseHttpsRedirection();

    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseMiddleware<LogContextMiddleware>();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (HostAbortedException) { }
catch (Exception ex)
{
    await File.AppendAllTextAsync("/home/monavixx/log-this-shit", ex.ToString());
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static async Task AddInfrastructureServices(WebApplicationBuilder builder)
{
    string redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? string.Empty;
    if (!string.IsNullOrEmpty(redisConnectionString))
    {
        try
        {
            var conf = ConfigurationOptions.Parse(redisConnectionString);
            conf.AbortOnConnectFail = false;
            var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(conf);
            builder.Services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to connect to Redis during startup; skipping Redis registration.");
        }
    }
    if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
    {
        var hangfireConnection = builder.Configuration.GetConnectionString("HangfireConnection");
        if (!string.IsNullOrEmpty(hangfireConnection))
        {
            builder.Services
                .AddHangfire(c => c
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(a =>
                        a.UseNpgsqlConnection(hangfireConnection))
                ).AddHangfireServer(c => c.WorkerCount = 2)
                .AddScoped<IBackgroundJobService, BackgroundJobService>();
        }
    }
    builder.Services
        .AddHttpContextAccessor()
        .AddScoped<ICurrentUserService, CurrentUserService>()
        .AddScoped<IReadOnlyCurrentUserService, ReadOnlyCurrentUserService>()
        .AddSingleton(TimeProvider.System);
    builder.Services.AddDbContextFactory<AppDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention();
    });
}

static void AddCoreServices(WebApplicationBuilder builder)
{
    builder.Services.AddSerilog((services, s) =>
    {
        s.ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(LogEventLevel.Debug, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}){NewLine}{Message:lj}{NewLine}{Exception}");

        if (!builder.Environment.IsEnvironment("Test"))
        {
            s.WriteTo.Seq("http://localhost:5341");
            s.ReadFrom.Configuration(builder.Configuration);
        }
    })
        .AddSingleton<BookingDurationCalculator>()
        .AddOptions<BookingOptions>()
            .Bind(builder.Configuration.GetSection(BookingOptions.SectionName))
            .ValidateOnStart();
    builder.Services.AddMediatR(c =>
    {
        c.RegisterServicesFromAssembly(typeof(AppDbContext).Assembly);
        c.AddOpenBehavior(typeof(ValidationBehavior<,>));
        c.AddOpenBehavior(typeof(ActiveUserCheckBehavior<,>));
        c.AddOpenBehavior(typeof(LoggingBehavior<,>));
        c.AddOpenBehavior(typeof(DbExceptionHandlingBehavior<,>));
    });
    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
    builder.Services.AddOpenApi();

    builder.Services.AddProblemDetails();
    builder.Services.AddValidatorsFromAssembly(typeof(AppDbContext).Assembly);
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var opt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
            options.RequireHttpsMetadata = true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = opt.Issuer,
                ValidAudience = opt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opt.Secret)),
                ClockSkew = TimeSpan.Zero,
                NameClaimType = JwtRegisteredClaimNames.Sub,
                RoleClaimType = "role"
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.TryGetValue("access_token", out var token))
                    {
                        context.Token = token;
                    }

                    return Task.CompletedTask;
                }
            };
        });
    builder.Services.AddAuthorizationBuilder();
    builder.Services.AddOptions<RefreshTokenOptions>()
        .Bind(builder.Configuration.GetSection(RefreshTokenOptions.SectionName))
        .ValidateOnStart();
    builder.Services.AddOptions<JwtOptions>()
        .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
        .ValidateOnStart();
    builder.Services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
    builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
    builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>()
        .AddSingleton<ConstraintErrorRegistryBase>(sp =>
            {
                var factory = sp.GetService<IDbContextFactory<AppDbContext>>();
                if (factory is null) return new ConstraintErrorRegistry();

                using var db = factory.CreateDbContext();

                var cer = new ConstraintErrorRegistry(db.Model);
                cer.AddConstraintErrorsFromAssembly(typeof(AppDbContext).Assembly);

                return cer;
            }
        )
        .AddScoped<IBookingCancellationService, BookingCancellationService>()
        .AddScoped<IBookingCompletionService, BookingCompletionService>()
        .AddScoped<IUserBlocker, UserBlocker>()
        .AddScoped<IUserStore, UserStore>()
        .AddSingleton<IUserCache, RedisUserCache>()
        .AddScoped<IClaimsTransformation, RoleClaimsTransformation>();
}
