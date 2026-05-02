using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Users.Commands.SignUp;

public sealed record SignUpCommand(
    string Username,
    string Password,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateOnly DateOfBirth
) : IRequest<Result<SuccessfulSignUpResult>>;