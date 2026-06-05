using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using BookingSystem.Api;
using BookingSystem.Api.Middlewares;
using BookingSystem.Api.Services;
using BookingSystem.Application;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Infrastructure;
using BookingSystem.Infrastructure.Options;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(LogEventLevel.Debug)
    .WriteTo.Seq("http://localhost:5341")
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    // Disable legacy WS-Federation claim type mapping.
    // Without this, standard JWT claims like "sub" get remapped to long URI-based
    // claim types (e.g., sub → NameIdentifier URI), which breaks direct claim lookups.
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
    JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
    
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, s) => s
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(LogEventLevel.Debug, outputTemplate:"[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}, {Properties}) {Message:lj}{NewLine}{Exception}")
        .WriteTo.Seq("http://localhost:5341")
    );

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
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<IClaimsTransformation, RoleClaimsTransformation>();

    var app = builder.Build();


    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    
    app.UseMiddleware<LogContextMiddleware>();
    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch(HostAbortedException){}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}