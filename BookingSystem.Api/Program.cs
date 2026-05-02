using System.Text;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.PipelineBehaviors;
using BookingSystem.Application.Persistence;
using BookingSystem.Infrastructure;
using BookingSystem.Infrastructure.Options;
using BookingSystem.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(typeof(AppDbContext).Assembly);
    configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssembly(typeof(AppDbContext).Assembly);
builder.Services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
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
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("access_token", out var token))
                {
                    context.Token = token;
                    // return Task.CompletedTask;
                }

                // var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                // if (authHeader?.StartsWith("Bearer ") is true)
                // {
                //     context.Token = authHeader["Bearer ".Length..].Trim();
                // }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();