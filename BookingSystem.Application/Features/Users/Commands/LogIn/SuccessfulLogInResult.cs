using BookingSystem.Application.Common.DTOs;

namespace BookingSystem.Application.Features.Users.Commands.LogIn;

public sealed record SuccessfulLogInResult
{
    public Guid Id { get; init; }
    public AuthTokens AuthTokens { get; init; } = null!;
}