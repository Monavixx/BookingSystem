using BookingSystem.Application.Common.Attributes;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Users.Commands.LogIn;

public sealed record LogInCommand (
    [property: SensitiveCommandProperty] string? Email, 
    string? Username,
    [property: SensitiveCommandProperty] string Password
    ) : IRequest<Result<SuccessfulLogInResult>>;