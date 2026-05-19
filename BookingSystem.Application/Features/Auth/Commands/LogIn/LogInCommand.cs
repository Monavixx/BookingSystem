using BookingSystem.Application.Common.Attributes;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Auth.Commands.LogIn;

public sealed record LogInCommand (
    [property: SensitiveCommandProperty] string? Email, 
    string? Username,
    [property: SensitiveCommandProperty] string Password
    ) : IRequest<Result<SuccessfulLogInResult>>;