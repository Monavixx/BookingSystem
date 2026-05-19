using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.DTOs;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Common.ValueObjects;
using BookingSystem.Domain.User;
using BookingSystem.Domain.User.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Auth.Commands.LogIn;

public class LogInHandler(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IRefreshTokenService refreshTokenService,
    IJwtTokenService jwtTokenService,
    ILogger<LogInHandler> logger)
    : IRequestHandler<LogInCommand, Result<SuccessfulLogInResult>>
{
    public async Task<Result<SuccessfulLogInResult>> Handle(LogInCommand request, CancellationToken cancellationToken)
    {
        User? user = null;
        switch (request)
        {
            case { Email: null, Username: { } usernameStr }:
                if (Username.Create(usernameStr).ValueOrDefault is not { } username)
                {
                    logger.LogWarning("Login failed: invalid username format");
                    return Result.Fail<SuccessfulLogInResult>(LogInErrors.InvalidCredentials);
                }
                logger.LogInformation("Login attempt by username");
                user = await dbContext.Users.FirstOrDefaultAsync(
                    u => u.Username == username,
                    cancellationToken: cancellationToken);
                break;

            case { Email: { } emailStr, Username: null }:
                if (EmailAddress.Create(emailStr).ValueOrDefault is not { } email)
                {
                    logger.LogWarning("Login failed: invalid email format");
                    return Result.Fail<SuccessfulLogInResult>(LogInErrors.InvalidCredentials);
                }
                logger.LogInformation("Login attempt by email");
                user = await dbContext.Users.FirstOrDefaultAsync(
                    u => u.Email == email,
                    cancellationToken: cancellationToken);
                break;

            case { Email: null, Username: null }:
                logger.LogWarning("Login failed: no identifier provided");
                return Result.Fail<SuccessfulLogInResult>(LogInErrors.IdentifierMissing);

            case { Email: not null, Username: not null }:
                logger.LogWarning("Login failed: both email and username provided");
                return Result.Fail<SuccessfulLogInResult>(LogInErrors.IdentifierAmbiguous);
        }

        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            logger.LogInformation("Failed login attempt: user not found or invalid password");
            return Result.Fail<SuccessfulLogInResult>(LogInErrors.InvalidCredentials);
        }
        
        var rt = refreshTokenService.GenerateRefreshToken();
        user.AddSession(rt);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("User {UserId} logged in successfully", user.Id.Value);
        
        return Result.Ok(new SuccessfulLogInResult()
        {
            Id = user.Id.Value,
            AuthTokens = new AuthTokens(jwtTokenService.GenerateJwtToken(user), rt)
        });
    }
}