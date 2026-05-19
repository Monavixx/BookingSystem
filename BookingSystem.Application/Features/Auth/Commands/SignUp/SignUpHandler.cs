using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.DTOs;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.User;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Auth.Commands.SignUp;

public class SignUpHandler(
    IPasswordHasher passwordHasher,
    AppDbContext appDbContext,
    IRefreshTokenService refreshTokenService,
    IJwtTokenService jwtTokenService,
    ILogger<SignUpHandler> logger)
    : IRequestHandler<SignUpCommand, Result<SuccessfulSignUpResult>>
{
    public async Task<Result<SuccessfulSignUpResult>> Handle(SignUpCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Trying to sign up user {UserName}", request.Username);
        var result = User.Create(
            username: request.Username,
            email: request.Email,
            phoneNumber: request.PhoneNumber,
            passwordHash: passwordHasher.HashPassword(request.Password),
            birthdate: request.DateOfBirth,
            firstName: request.FirstName,
            lastName: request.LastName
        );
        
        if (result.IsFailed)
        {
            logger.LogWarning("User creation failed for {UserName}: {@Errors}", request.Username, result.Errors);
            return Result.Fail<SuccessfulSignUpResult>(result.Errors);
        }

        var user = result.Value;
        appDbContext.Users.Add(user);

        var rt = refreshTokenService.GenerateRefreshToken();
        user.AddSession(rt);
        var jwtToken = jwtTokenService.GenerateJwtToken(user);
        
        await appDbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User {UserName} signed up successfully with id {UserId}", request.Username, user.Id);

        return Result.Ok(new SuccessfulSignUpResult()
        {
            Id = user.Id.Value,
            AuthTokens = new AuthTokens(AccessToken: jwtToken,
                RefreshToken: rt
            )
        });
    }
}