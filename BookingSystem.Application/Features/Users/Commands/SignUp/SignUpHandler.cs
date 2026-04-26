using BookingSystem.Application.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.User;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Users.Commands.SignUp;

public class SignUpHandler(IPasswordHasher passwordHasher, AppDbContext appDbContext)
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
        
        appDbContext.Users.Add(result.Value);
        await appDbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(new SuccessfulSignUpResultDto()
        {
            Id = result.Value.Id.Value,
        });
    }
}