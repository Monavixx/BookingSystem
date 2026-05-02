using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.DTOs;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.User;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Users.Commands.SignUp;

public class SignUpHandler(
    IPasswordHasher passwordHasher,
    AppDbContext appDbContext,
    IRefreshTokenService refreshTokenService,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<SignUpCommand, Result<SuccessfulSignUpResultDto>>
{
    public async Task<Result<SuccessfulSignUpResultDto>> Handle(SignUpCommand request,
        CancellationToken cancellationToken)
    {
        
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
            return Result.Fail<SuccessfulSignUpResultDto>(result.Errors);
        }

        var user = result.Value;

        appDbContext.Users.Add(user);

        var rt = refreshTokenService.GenerateRefreshToken();
        user.AddSession(rt);
        
        await appDbContext.SaveChangesAsync(cancellationToken);
        
        var jwtToken = jwtTokenService.GenerateJwtToken(user);

        return Result.Ok(new SuccessfulSignUpResultDto()
        {
            Id = user.Id.Value,
            AuthTokens = new AuthTokens(AccessToken: jwtToken,
                RefreshToken: rt
            )
        });
    }
}