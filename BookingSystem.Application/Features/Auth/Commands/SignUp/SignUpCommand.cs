using BookingSystem.Application.Common.Attributes;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Auth.Commands.SignUp;

public sealed record SignUpCommand(
    string Username,
    [property: SensitiveCommandProperty] string Password,
    [property: SensitiveCommandProperty] string Email,
    [property: SensitiveCommandProperty] string FirstName,
    [property: SensitiveCommandProperty] string LastName,
    [property: SensitiveCommandProperty] string PhoneNumber,
    [property: SensitiveCommandProperty] DateOnly DateOfBirth
) : IRequest<Result<SuccessfulSignUpResult>>;