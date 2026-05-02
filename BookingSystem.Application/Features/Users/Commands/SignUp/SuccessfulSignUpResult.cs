using BookingSystem.Application.Common.DTOs;

namespace BookingSystem.Application.Features.Users.Commands.SignUp;

public sealed record SuccessfulSignUpResult
{
    public Guid Id { get; init; }
    public AuthTokens AuthTokens { get; init; } = null!;
}