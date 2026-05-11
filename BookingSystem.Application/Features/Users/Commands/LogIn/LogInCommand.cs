using BookingSystem.Domain.User;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Users.Commands.LogIn;

public sealed record LogInCommand (string? Email, string? Username, string Password) 
    : IRequest<Result<SuccessfulLogInResult>>;