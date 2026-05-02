using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.DTOs;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Common.ValueObjects;
using BookingSystem.Domain.User;
using BookingSystem.Domain.User.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Features.Users.Commands.LogIn;

public class LogInHandler(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IRefreshTokenService refreshTokenService,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LogInCommand, Result<SuccessfulLogInResult>>
{
    public async Task<Result<SuccessfulLogInResult>> Handle(LogInCommand request, CancellationToken cancellationToken)
    {
        User? user = null;
        switch (request)
        {
            case { Email: null, Username: { } usernameStr }
                when Username.Create(usernameStr).ValueOrDefault is { } username:

                user = await dbContext.Users.FirstOrDefaultAsync(
                    u => u.Username == username,
                    cancellationToken: cancellationToken);
                break;

            case { Email: { } emailStr, Username: null } when
                EmailAddress.Create(emailStr).ValueOrDefault is { } email:

                user = await dbContext.Users.FirstOrDefaultAsync(
                    u => u.Email == email,
                    cancellationToken: cancellationToken);
                break;

            case { Email: null, Username: null }:
                return Result.Fail<SuccessfulLogInResult>(LogInErrors.IdentifierMissing);

            case { Email: not null, Username: not null }:
                return Result.Fail<SuccessfulLogInResult>(LogInErrors.IdentifierAmbiguous);
        }

        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return Result.Fail<SuccessfulLogInResult>(LogInErrors.InvalidCredentials);
        
        var rt = refreshTokenService.GenerateRefreshToken();
        user.AddSession(rt);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok(new SuccessfulLogInResult()
        {
            Id = user.Id.Value,
            AuthTokens = new AuthTokens(jwtTokenService.GenerateJwtToken(user), rt)
        });
    }
}