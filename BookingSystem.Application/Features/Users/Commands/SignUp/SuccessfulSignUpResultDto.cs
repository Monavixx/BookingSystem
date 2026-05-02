using BookingSystem.Application.Common.DTOs;

namespace BookingSystem.Application.Features.Users.Commands.SignUp;

public sealed record SuccessfulSignUpResultDto
{
    public Guid Id { get; init; }
    public AuthTokens AuthTokens { get; init; } = null!;
}